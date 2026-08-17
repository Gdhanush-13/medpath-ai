using MedPath.Domain;

namespace MedPath.Application;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, UserDto User);
public sealed record UserDto(Guid Id, string Email, string DisplayName, string[] Roles, string Status);
public sealed record CreateCourseRequest(string Title, string Description);
public sealed record CreateModuleRequest(string Title);
public sealed record CreateLessonRequest(string Title, string Content);
public sealed record CreateQuestionRequest(string Prompt, IReadOnlyList<QuestionOptionRequest> Options);
public sealed record QuestionOptionRequest(string Text, bool IsCorrect);
public sealed record CourseSummaryDto(Guid Id, string Title, string Description, string Status, int ModuleCount, int EnrollmentCount);
public sealed record CourseDetailsDto(Guid Id, string Title, string Description, string Status, IReadOnlyList<ModuleDto> Modules);
public sealed record ModuleDto(Guid Id, string Title, int SortOrder, IReadOnlyList<LessonDto> Lessons, IReadOnlyList<AssessmentDto> Assessments);
public sealed record LessonDto(Guid Id, string Title, string Content, bool Completed);
public sealed record AssessmentDto(Guid Id, string Title, IReadOnlyList<QuestionDto> Questions);
public sealed record QuestionDto(Guid Id, string Prompt, IReadOnlyList<OptionDto> Options);
public sealed record OptionDto(Guid Id, string Text);
public sealed record SubmitAssessmentRequest(IReadOnlyDictionary<Guid, Guid> Answers);
public sealed record AssessmentResultDto(Guid AttemptId, int Score, int TotalQuestions, DateTime SubmittedAtUtc);
public sealed record ProgressDto(Guid CourseId, int CompletedLessons, int TotalLessons, decimal CompletionPercent);
public sealed record DashboardDto(int EnrolledCourses, int CompletedLessons, int AssessmentsTaken, decimal AverageScore, IReadOnlyList<CourseSummaryDto> Courses);
public sealed record CreateUserRequest(string Email, string DisplayName, string Password, string Role);
public sealed record UpdateUserStatusRequest(bool Active);
public sealed record AiStudyRequest(string Action, string LessonTitle, string LessonContent);
public sealed record AiStudyResponse(string Action, string Answer, bool IsEducationalOnly);

public interface ITokenService
{
    Task<AuthResponse> IssueAsync(AppUser user, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken);
}

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IAiStudyService
{
    Task<AiStudyResponse> AskAsync(AiStudyRequest request, CancellationToken cancellationToken);
}

public interface IAuditService
{
    Task WriteAsync(Guid? actorId, string? actorEmail, AuditAction action, string? targetType, Guid? targetId, object? metadata, CancellationToken cancellationToken);
}
