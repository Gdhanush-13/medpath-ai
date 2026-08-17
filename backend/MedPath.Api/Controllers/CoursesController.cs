using System.Security.Claims;
using MedPath.Application;
using MedPath.Domain;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Route("api/courses")]
public sealed class CoursesController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var courses = await db.Courses.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Select(x => new CourseSummaryDto(x.Id, x.Title, x.Description, x.Status.ToString(), x.Modules.Count, x.Enrollments.Count)).ToListAsync(cancellationToken);
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDetailsDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.AsNoTracking().Include(x => x.Modules).ThenInclude(x => x.Lessons).Include(x => x.Modules).ThenInclude(x => x.Assessments).ThenInclude(x => x.Questions).ThenInclude(x => x.Options).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (course is null) return NotFound();
        var completed = User.Identity?.IsAuthenticated == true ? await db.LearningProgress.Where(x => x.Enrollment!.StudentId == UserId()).Select(x => x.LessonId).ToListAsync(cancellationToken) : [];
        return Ok(new CourseDetailsDto(course.Id, course.Title, course.Description, course.Status.ToString(), course.Modules.OrderBy(x => x.SortOrder).Select(module => new ModuleDto(module.Id, module.Title, module.SortOrder, module.Lessons.OrderBy(x => x.SortOrder).Select(lesson => new LessonDto(lesson.Id, lesson.Title, lesson.Content, completed.Contains(lesson.Id))).ToArray(), module.Assessments.Select(assessment => new AssessmentDto(assessment.Id, assessment.Title, assessment.Questions.OrderBy(x => x.SortOrder).Select(q => new QuestionDto(q.Id, q.Prompt, q.Options.Select(o => new OptionDto(o.Id, o.Text)).ToArray())).ToArray())).ToArray())).ToArray()));
    }

    [Authorize(Policy = "CourseManagement"), HttpPost]
    public async Task<ActionResult<CourseSummaryDto>> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { message = "Title is required." });
        var course = new Course { Title = request.Title.Trim(), Description = request.Description.Trim(), OrganizationId = await OrganizationId(cancellationToken), CreatedById = UserId() };
        db.Courses.Add(course); await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.CourseCreated, "Course", course.Id, null, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = course.Id }, new CourseSummaryDto(course.Id, course.Title, course.Description, course.Status.ToString(), 0, 0));
    }

    [Authorize(Policy = "CourseManagement"), HttpPost("{id:guid}/modules")]
    public async Task<IActionResult> AddModule(Guid id, CreateModuleRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Courses.AnyAsync(x => x.Id == id, cancellationToken)) return NotFound();
        var module = new CourseModule { CourseId = id, Title = request.Title.Trim(), SortOrder = await db.Modules.CountAsync(x => x.CourseId == id, cancellationToken) + 1 }; db.Modules.Add(module); await db.SaveChangesAsync(cancellationToken); return Ok(new { module.Id });
    }

    [Authorize(Policy = "CourseManagement"), HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (course is null) return NotFound(); course.Status = CourseStatus.Published; course.PublishedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.CoursePublished, "Course", id, null, cancellationToken); return NoContent();
    }

    [Authorize(Policy = "CourseManagement"), HttpPost("{id:guid}/enrollments/{studentId:guid}")]
    public async Task<IActionResult> Enroll(Guid id, Guid studentId, CancellationToken cancellationToken)
    {
        if (!await db.Courses.AnyAsync(x => x.Id == id, cancellationToken) || !await db.Users.AnyAsync(x => x.Id == studentId, cancellationToken)) return NotFound();
        if (!await db.Enrollments.AnyAsync(x => x.CourseId == id && x.StudentId == studentId, cancellationToken)) { db.Enrollments.Add(new Enrollment { CourseId = id, StudentId = studentId }); await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.CourseAssigned, "Course", id, new { studentId }, cancellationToken); }
        return NoContent();
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private string? UserEmail() => User.FindFirstValue(ClaimTypes.Email);
    private async Task<Guid> OrganizationId(CancellationToken ct) => (await db.Users.AsNoTracking().SingleAsync(x => x.Id == UserId(), ct)).OrganizationId;
}
