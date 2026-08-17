using System.Security.Claims;
using MedPath.Application;
using MedPath.Domain;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(AppDbContext db, IPasswordService passwords, ITokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Email == request.Email.ToLower(), cancellationToken);
        if (user is null || user.Status != UserStatus.Active || !passwords.Verify(request.Password, user.PasswordHash)) return Unauthorized(new { message = "Invalid email or password." });
        var roles = user.UserRoles.Where(x => x.Role is not null).Select(x => x.Role!.Name).ToArray();
        return Ok(await tokens.IssueAsync(user, roles, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken) => (await tokens.RefreshAsync(request.RefreshToken, cancellationToken)) is { } response ? Ok(response) : Unauthorized(new { message = "Refresh token is invalid or expired." });

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken) { await tokens.RevokeAsync(UserId(), request.RefreshToken, cancellationToken); return NoContent(); }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == UserId(), cancellationToken);
        return user is null ? NotFound() : Ok(TokenService.ToDto(user, user.UserRoles.Where(x => x.Role is not null).Select(x => x.Role!.Name).ToArray()));
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
