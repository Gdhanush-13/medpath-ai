namespace MedPath.Domain;

public enum UserStatus { Active, Inactive }
public enum CourseStatus { Draft, Published, Archived }
public enum AuditAction { UserCreated, UserRoleChanged, UserDeactivated, CourseCreated, CoursePublished, CourseAssigned, AssessmentSubmitted }

public sealed class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<AppUser> Users { get; set; } = [];
    public ICollection<Course> Courses { get; set; } = [];
}

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Course> CoursesCreated { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public sealed class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}

public sealed class UserRole
{
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

public sealed class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid CreatedById { get; set; }
    public AppUser? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; set; }
    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public sealed class CourseModule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public required string Title { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Assessment> Assessments { get; set; } = [];
}

public sealed class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public CourseModule? Module { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public int SortOrder { get; set; }
}

public sealed class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public Guid StudentId { get; set; }
    public AppUser? Student { get; set; }
    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<LearningProgress> Progress { get; set; } = [];
}

public sealed class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public CourseModule? Module { get; set; }
    public required string Title { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<AssessmentAttempt> Attempts { get; set; } = [];
}

public sealed class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }
    public required string Prompt { get; set; }
    public int SortOrder { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = [];
}

public sealed class QuestionOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}

public sealed class AssessmentAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }
    public Guid StudentId { get; set; }
    public AppUser? Student { get; set; }
    public int Score { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<AssessmentAnswer> Answers { get; set; } = [];
}

public sealed class AssessmentAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttemptId { get; set; }
    public AssessmentAttempt? Attempt { get; set; }
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
}

public sealed class LearningProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public Guid LessonId { get; set; }
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public required string TokenHash { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public AuditAction Action { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
