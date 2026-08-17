using MedPath.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPath.Api.Controllers;

[ApiController, Authorize(Policy = "StudentLearning"), Route("api/ai-study")]
public sealed class AiStudyController(IAiStudyService ai) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AiStudyResponse>> Ask(AiStudyRequest request, CancellationToken cancellationToken) => Ok(await ai.AskAsync(request, cancellationToken));
}
