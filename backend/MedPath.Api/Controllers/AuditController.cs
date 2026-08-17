using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Authorize(Policy = "UserManagement"), Route("api/audit-logs")]
public sealed class AuditController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await db.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(100).Select(x => new { x.Id, x.ActorEmail, action = x.Action.ToString(), x.TargetType, x.TargetId, x.CreatedAtUtc }).ToListAsync(cancellationToken));
}
