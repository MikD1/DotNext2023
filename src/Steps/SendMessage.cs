using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace VacationBot.Steps;

public class SendMessage : IStepBody
{
    public SendMessage(TelegramBotService botClient)
    {
        _botService = botClient;
    }

    public string UserId { get; set; } = default!;

    public string Message { get; set; } = default!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        await _botService.SendMessage(UserId, Message);
        return ExecutionResult.Next();
    }

    private readonly TelegramBotService _botService;
}