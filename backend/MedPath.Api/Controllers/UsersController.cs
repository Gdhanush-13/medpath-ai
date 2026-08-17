using System.Security.Claims;
using MedPath.Application;
using MedPath.Domain;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Authorize(Policy = "UserManagement"), Route("api/users")]
public sealed class UsersController(AppDbContext db, IPasswordService passwords, IAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role).OrderBy(x => x.DisplayName).ToListAsync(cancellationToken); return Ok(users.Select(x => TokenService.ToDto(x, x.UserRoles.Where(r => r.Role is not null).Select(r => r.Role!.Name).ToArray())));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == request.Role, cancellationToken); if (role is null) return BadRequest(new { message = "Unknown role." }); if (await db.Users.AnyAsync(x => x.Email == request.Email.ToLower(), cancellationToken)) return Conflict(new { message = "Email already exists." });
        var user = new AppUser { Email = request.Email.Trim().ToLowerInvariant(), DisplayName = request.DisplayName.Trim(), PasswordHash = passwords.Hash(request.Password), OrganizationId = await CurrentOrg(cancellationToken) }; user.UserRoles.Add(new UserRole { User = user, Role = role }); db.Users.Add(user); await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.UserCreated, "User", user.Id, new { request.Role }, cancellationToken); return Ok(new UserDto(user.Id, user.Email, user.DisplayName, [role.Name], user.Status.ToString()));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (user is null) return NotFound(); user.Status = request.Active ? UserStatus.Active : UserStatus.Inactive; await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.UserDeactivated, "User", id, new { request.Active }, cancellationToken); return NoContent();
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private string? UserEmail() => User.FindFirstValue(ClaimTypes.Email);
    private async Task<Guid> CurrentOrg(CancellationToken ct) => (await db.Users.AsNoTracking().SingleAsync(x => x.Id == UserId(), ct)).OrganizationId;
}
