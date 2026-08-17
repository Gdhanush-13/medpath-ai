using System.Security.Claims;
using MedPath.Domain;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Authorize, Route("api/analytics")]
public sealed class AnalyticsController(AppDbContext db) : ControllerBase
{
    [HttpGet("student")]
    public async Task<IActionResult> Student(CancellationToken cancellationToken)
    {
        var id = UserId(); var attempts = await db.AssessmentAttempts.AsNoTracking().Where(x => x.StudentId == id).ToListAsync(cancellationToken); var lessons = await db.LearningProgress.AsNoTracking().CountAsync(x => x.Enrollment!.StudentId == id, cancellationToken); return Ok(new { completedLessons = lessons, assessments = attempts.Count, averageScore = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(x => x.Score), 1) });
    }

    [Authorize(Policy = "CourseManagement"), HttpGet("educator")]
    public async Task<IActionResult> Educator(CancellationToken cancellationToken) => Ok(new { courses = await db.Courses.CountAsync(x => x.CreatedById == UserId(), cancellationToken), learners = await db.Enrollments.CountAsync(x => x.Course!.CreatedById == UserId(), cancellationToken), published = await db.Courses.CountAsync(x => x.CreatedById == UserId() && x.Status == CourseStatus.Published, cancellationToken) });

    [Authorize(Policy = "UserManagement"), HttpGet("admin")]
    public async Task<IActionResult> Admin(CancellationToken cancellationToken) => Ok(new { users = await db.Users.CountAsync(cancellationToken), students = await db.UserRoles.CountAsync(x => x.Role!.Name == "Student", cancellationToken), educators = await db.UserRoles.CountAsync(x => x.Role!.Name == "Educator", cancellationToken), courses = await db.Courses.CountAsync(cancellationToken), auditEvents = await db.AuditLogs.CountAsync(cancellationToken) });
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
