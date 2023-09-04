using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;

namespace VacationBot.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowController : ControllerBase
{
    public WorkflowController(IWorkflowHost workflowHost)
    {
        _workflowHost = workflowHost;
    }

    [HttpPost("accept")]
    public async Task<ActionResult> Accept(string userId)
    {
        await _workflowHost.PublishEvent("VacationReviewed", userId, "OK", DateTime.UtcNow);
        return Ok();
    }

    [HttpPost("decline")]
    public async Task<ActionResult> Decline(string userId, string reason)
    {
        await _workflowHost.PublishEvent("VacationReviewed", userId, reason, DateTime.UtcNow);
        return Ok();
    }

    private readonly IWorkflowHost _workflowHost;
}
