using VacationBot;
using VacationBot.Steps;
using VacationBot.Workflows;
using WorkflowCore.Interface;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TelegramBotService>();
builder.Services.AddHostedService(provider => provider.GetService<TelegramBotService>()!);
AddWorkflow(builder);

WebApplication app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
StartWorkflowHost(app);
app.Run();

void AddWorkflow(WebApplicationBuilder builder)
{
    builder.Services.AddTransient<SendMessage>();
    builder.Services.AddTransient<SendVariants>();
    builder.Services.AddWorkflow();
}

void StartWorkflowHost(WebApplication app)
{
    IWorkflowHost host = app.Services.GetRequiredService<IWorkflowHost>();
    host.RegisterWorkflow<VacationWorkflow, VacationWorkflowData>();
    host.RegisterWorkflow<VacationWorkflow2, VacationWorkflowData>();
    host.Start();
}