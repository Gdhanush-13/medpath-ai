using System.Security.Claims;
using MedPath.Application;
using MedPath.Domain;
using MedPath.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Api.Controllers;

[ApiController, Authorize(Policy = "StudentLearning"), Route("api/learning")]
public sealed class LearningController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken cancellationToken)
    {
        var studentId = UserId();
        var enrollments = await db.Enrollments.AsNoTracking().Include(x => x.Course).ThenInclude(x => x!.Modules).Include(x => x.Progress).Where(x => x.StudentId == studentId).ToListAsync(cancellationToken);
        var attempts = await db.AssessmentAttempts.AsNoTracking().Where(x => x.StudentId == studentId).ToListAsync(cancellationToken);
        var total = enrollments.Sum(x => x.Course?.Modules.Sum(m => m.Lessons.Count) ?? 0); var completed = enrollments.Sum(x => x.Progress.Count);
        return Ok(new DashboardDto(enrollments.Count, completed, attempts.Count, attempts.Count == 0 ? 0 : Math.Round((decimal)attempts.Average(x => x.Score), 1), enrollments.Where(x => x.Course is not null).Select(x => new CourseSummaryDto(x.Course!.Id, x.Course.Title, x.Course.Description, x.Course.Status.ToString(), x.Course.Modules.Count, x.Course.Enrollments.Count)).ToArray()));
    }

    [HttpGet("course/{courseId:guid}")]
    public async Task<ActionResult<ProgressDto>> Progress(Guid courseId, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments.Include(x => x.Course).ThenInclude(x => x!.Modules).ThenInclude(x => x.Lessons).Include(x => x.Progress).SingleOrDefaultAsync(x => x.CourseId == courseId && x.StudentId == UserId(), cancellationToken);
        if (enrollment?.Course is null) return NotFound(); var total = enrollment.Course.Modules.Sum(x => x.Lessons.Count); var done = enrollment.Progress.Count; return Ok(new ProgressDto(courseId, done, total, total == 0 ? 0 : Math.Round(done * 100m / total, 1)));
    }

    [HttpPost("lessons/{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments.Include(x => x.Course).ThenInclude(x => x!.Modules).ThenInclude(x => x.Lessons).SingleOrDefaultAsync(x => x.StudentId == UserId() && x.Course!.Modules.Any(m => m.Lessons.Any(l => l.Id == lessonId)), cancellationToken);
        if (enrollment is null) return NotFound(); if (!await db.LearningProgress.AnyAsync(x => x.EnrollmentId == enrollment.Id && x.LessonId == lessonId, cancellationToken)) { db.LearningProgress.Add(new LearningProgress { EnrollmentId = enrollment.Id, LessonId = lessonId }); await db.SaveChangesAsync(cancellationToken); }
        return NoContent();
    }

    [HttpPost("assessments/{assessmentId:guid}/submit")]
    public async Task<ActionResult<AssessmentResultDto>> Submit(Guid assessmentId, SubmitAssessmentRequest request, CancellationToken cancellationToken)
    {
        var assessment = await db.Assessments.Include(x => x.Questions).ThenInclude(x => x.Options).SingleOrDefaultAsync(x => x.Id == assessmentId, cancellationToken); if (assessment is null) return NotFound();
        var answers = assessment.Questions.Select(q => new AssessmentAnswer { QuestionId = q.Id, SelectedOptionId = request.Answers.TryGetValue(q.Id, out var option) ? option : Guid.Empty, IsCorrect = q.Options.Any(o => o.Id == request.Answers.GetValueOrDefault(q.Id) && o.IsCorrect) }).ToList();
        var attempt = new AssessmentAttempt { AssessmentId = assessmentId, StudentId = UserId(), Score = answers.Count(x => x.IsCorrect) * 100 / Math.Max(1, assessment.Questions.Count), Answers = answers }; db.AssessmentAttempts.Add(attempt); await db.SaveChangesAsync(cancellationToken); await audit.WriteAsync(UserId(), UserEmail(), AuditAction.AssessmentSubmitted, "Assessment", assessmentId, new { attempt.Score }, cancellationToken); return Ok(new AssessmentResultDto(attempt.Id, attempt.Score, assessment.Questions.Count, attempt.SubmittedAtUtc));
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private string? UserEmail() => User.FindFirstValue(ClaimTypes.Email);
}
