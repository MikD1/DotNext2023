using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace VacationBot.Steps;

public class SendVariants : IStepBody
{
    public SendVariants(TelegramBotService botService)
    {
        _botService = botService;
    }

    public string UserId { get; set; } = default!;

    public string Message { get; set; } = default!;

    public string[] Variants { get; set; } = default!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        await _botService.SendVariants(UserId, Message, Variants);
        return ExecutionResult.Next();
    }

    private readonly TelegramBotService _botService;
}