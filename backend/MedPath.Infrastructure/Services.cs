using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MedPath.Application;
using MedPath.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedPath.Infrastructure;

public sealed class PasswordService : IPasswordService
{
    private const int Iterations = 120_000;
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public sealed class TokenService(AppDbContext db, IConfiguration configuration) : ITokenService
{
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "MedPathAI";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "MedPathAI.Web";
    private readonly string _key = configuration["Jwt:Key"] ?? "local-only-change-this-key-at-least-32-chars";

    public async Task<AuthResponse> IssueAsync(AppUser user, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(30);
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(ClaimTypes.Email, user.Email), new(ClaimTypes.Name, user.DisplayName) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_issuer, _audience, claims, now, expires, credentials);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        db.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = Hash(refresh), ExpiresAtUtc = now.AddDays(14) });
        await db.SaveChangesAsync(cancellationToken);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), refresh, expires, ToDto(user, roles));
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var entity = await db.RefreshTokens.Include(x => x.User).ThenInclude(x => x!.UserRoles).ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.TokenHash == Hash(refreshToken) && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);
        if (entity?.User is null || entity.User.Status != UserStatus.Active) return null;
        entity.RevokedAtUtc = DateTime.UtcNow;
        var roles = entity.User.UserRoles.Where(x => x.Role is not null).Select(x => x.Role!.Name).ToArray();
        await db.SaveChangesAsync(cancellationToken);
        return await IssueAsync(entity.User, roles, cancellationToken);
    }

    public async Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        var entity = await db.RefreshTokens.SingleOrDefaultAsync(x => x.UserId == userId && x.TokenHash == Hash(refreshToken), cancellationToken);
        if (entity is not null) { entity.RevokedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(cancellationToken); }
    }

    public static UserDto ToDto(AppUser user, IReadOnlyCollection<string> roles) => new(user.Id, user.Email, user.DisplayName, roles.ToArray(), user.Status.ToString());
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

public sealed class MockAiStudyService : IAiStudyService
{
    public Task<AiStudyResponse> AskAsync(AiStudyRequest request, CancellationToken cancellationToken)
    {
        var answer = request.Action.ToLowerInvariant() switch
        {
            "summarize" => $"Key learning points from {request.LessonTitle}: identify the main principle, the safe workflow, and the reason each step matters.",
            "questions" => $"Revision questions for {request.LessonTitle}: What is the core concept? Which safety checks matter? How would you explain it to a peer? What is a common mistake? How would you verify your understanding?",
            "flashcards" => $"Flashcards for {request.LessonTitle}: concept → definition; workflow → sequence; safety check → rationale; risk → mitigation; reflection → application.",
            _ => $"Here is a simple explanation of {request.LessonTitle}: start with the core idea, connect it to the lesson evidence, and check your understanding with a practical example."
        };
        return Task.FromResult(new AiStudyResponse(request.Action, answer, true));
    }
}

public sealed class AuditService(AppDbContext db) : IAuditService
{
    public async Task WriteAsync(Guid? actorId, string? actorEmail, AuditAction action, string? targetType, Guid? targetId, object? metadata, CancellationToken cancellationToken)
    {
        db.AuditLogs.Add(new AuditLog { ActorId = actorId, ActorEmail = actorEmail, Action = action, TargetType = targetType, TargetId = targetId, MetadataJson = JsonSerializer.Serialize(metadata ?? new { }) });
        await db.SaveChangesAsync(cancellationToken);
    }
}
