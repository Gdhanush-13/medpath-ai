using MedPath.Application;
using MedPath.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedPath.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, IPasswordService passwords, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken)) return;
        var organization = new Organization { Name = "MedPath Demo Institute" };
        var roles = new[] { new Role { Name = "Student" }, new Role { Name = "Educator" }, new Role { Name = "Administrator" } };
        db.Organizations.Add(organization); db.Roles.AddRange(roles);
        var admin = User("admin@medpath.local", "Demo Administrator", roles[2], organization, passwords);
        var educator = User("educator@medpath.local", "Demo Educator", roles[1], organization, passwords);
        var student = User("student@medpath.local", "Demo Student", roles[0], organization, passwords);
        db.Users.AddRange(admin, educator, student);
        var course = new Course { Title = "Patient Safety Essentials", Description = "Original educational examples for safe clinical learning.", Organization = organization, CreatedBy = educator, Status = CourseStatus.Published, PublishedAtUtc = DateTime.UtcNow };
        var module = new CourseModule { Course = course, Title = "Foundations of Safe Practice", SortOrder = 1 };
        module.Lessons.Add(new Lesson { Module = module, Title = "Patient Identification", Content = "Use appropriate identifiers and confirm the intended patient before a care activity.", SortOrder = 1 });
        module.Lessons.Add(new Lesson { Module = module, Title = "Safe Documentation", Content = "Record timely, objective and complete information according to local policy.", SortOrder = 2 });
        var assessment = new Assessment { Module = module, Title = "Safety Check" };
        var question = new Question { Assessment = assessment, Prompt = "What is the purpose of patient identification checks?", SortOrder = 1 };
        question.Options.Add(new QuestionOption { Question = question, Text = "To reduce preventable errors", IsCorrect = true });
        question.Options.Add(new QuestionOption { Question = question, Text = "To replace all clinical judgment", IsCorrect = false });
        assessment.Questions.Add(question); module.Assessments.Add(assessment); course.Modules.Add(module);
        db.Courses.Add(course);
        db.Enrollments.Add(new Enrollment { Course = course, Student = student });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static AppUser User(string email, string name, Role role, Organization organization, IPasswordService passwords)
    {
        var user = new AppUser { Email = email, DisplayName = name, PasswordHash = passwords.Hash("MedPath123!Local"), Organization = organization };
        user.UserRoles.Add(new UserRole { User = user, Role = role });
        return user;
    }
}
