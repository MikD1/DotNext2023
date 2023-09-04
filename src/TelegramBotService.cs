using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VacationBot.Workflows;
using WorkflowCore.Interface;

namespace VacationBot;

public class TelegramBotService : IHostedService
{
    public TelegramBotService(ILogger<TelegramBotService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        string token = _configuration["TelegramBotToken"]!;
        _botClient = new TelegramBotClient(token);
        _receivingCts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(HandleUpdate, HandleError, receiverOptions, _receivingCts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _receivingCts.Cancel();
        return Task.CompletedTask;
    }

    public async Task SendMessage(string userId, string message)
    {
        await _botClient.SendTextMessageAsync(userId, message, replyMarkup: new ReplyKeyboardRemove());
        _logger.LogInformation($"Message '{message}' send to user '{userId}'");
    }

    public async Task SendVariants(string userId, string message, string[] variants)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            variants.Select(x => new KeyboardButton(x))
        })
        {
            ResizeKeyboard = true
        };

        await _botClient.SendTextMessageAsync(userId, message, replyMarkup: replyKeyboardMarkup);
        _logger.LogInformation($"Variants message '{message}' send to user '{userId}'");
    }

    private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null)
        {
            return;
        }

        string userId = update.Message.Chat.Id.ToString();

        string? messageText = update.Message.Text;
        if (messageText is not null)
        {
            switch (messageText)
            {
                case "/start":
                    await StartWorkflowForUser(userId);
                    break;
                default:
                    await PublishWorkflowEvent("UserMessage", userId, messageText);
                    break;
            }

            _logger.LogInformation($"HandleTextUpdate: '{userId}' : '{messageText}'");
        }
    }

    private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    private async Task StartWorkflowForUser(string userId)
    {
        VacationWorkflowData data = new()
        {
            UserId = userId
        };

        using IServiceScope scope = _scopeFactory.CreateScope();
        IWorkflowHost workflowHost = scope.ServiceProvider.GetRequiredService<IWorkflowHost>();

        string workflowId = await workflowHost.StartWorkflow("VacationWorkflow2", data);
        _logger.LogInformation($"Workflow '{workflowId}' started for user '{userId}'");
    }

    private async Task PublishWorkflowEvent(string eventName, string userId, string? userText = null)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IWorkflowHost workflowHost = scope.ServiceProvider.GetRequiredService<IWorkflowHost>();
        await workflowHost.PublishEvent(eventName, userId, userText, DateTime.UtcNow);
    }

    private readonly ILogger<TelegramBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private ITelegramBotClient _botClient = default!;
    private CancellationTokenSource _receivingCts = default!;
}