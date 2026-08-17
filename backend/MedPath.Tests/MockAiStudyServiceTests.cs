using MedPath.Application;
using MedPath.Infrastructure;

namespace MedPath.Tests;

public sealed class MockAiStudyServiceTests
{
    [Fact]
    public async Task Mock_provider_returns_safe_marked_response()
    {
        var service = new MockAiStudyService();
        var response = await service.AskAsync(new AiStudyRequest("summarize", "Patient Safety", "lesson text"), CancellationToken.None);

        Assert.True(response.IsEducationalOnly);
        Assert.Equal("summarize", response.Action);
        Assert.Contains("Patient Safety", response.Answer);
    }
}
