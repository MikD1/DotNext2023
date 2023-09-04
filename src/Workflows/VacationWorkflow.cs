using VacationBot.Steps;
using WorkflowCore.Interface;

namespace VacationBot.Workflows;

public class VacationWorkflow : IWorkflow<VacationWorkflowData>
{
    public string Id => "VacationWorkflow";

    public int Version => 1;

    public void Build(IWorkflowBuilder<VacationWorkflowData> builder)
    {
        builder
            .StartWith<SendMessage>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, _ => "Привет! С помощью этого бота можно подать заявку на отпуск")

            .While(data => !data.IsVacationApproved).Do(_ => _
                .StartWith<SendMessage>()
                .Input(step => step.UserId, data => data.UserId)
                .Input(step => step.Message, _ => "Укажите дату начала отпуска")
                .WaitFor("UserMessage", data => data.UserId, _ => DateTime.UtcNow)
                .Output(data => data.StartDate, step => step.EventData)

                .Then<SendMessage>()
                .Input(step => step.UserId, data => data.UserId)
                .Input(step => step.Message, _ => "Укажите количество дней отпуска")
                .WaitFor("UserMessage", data => data.UserId, _ => DateTime.UtcNow)
                .Output(data => data.DaysAmount, step => int.Parse((string)step.EventData))

                .Then<SendVariants>()
                .Input(step => step.UserId, data => data.UserId)
                .Input(step => step.Message, _ => "Выберите тип отпуска")
                .Input(step => step.Variants, _ => new[] { "Основной", "Дополнительный", "Учебный", "Другой" })
                .WaitFor("UserMessage", data => data.UserId, _ => DateTime.UtcNow)
                .Output(data => data.VacationType, step => step.EventData)

                .If(data => data.DaysAmount > 14).Do(__ => __
                    .StartWith<SendMessage>()
                    .Input(step => step.UserId, data => data.UserId)
                    .Input(step => step.Message, _ => "Укажите вашего заместителя на время отпуска")
                    .WaitFor("UserMessage", data => data.UserId, _ => DateTime.UtcNow)
                    .Output(data => data.Deputy, step => step.EventData))

                .Then<SendMessage>()
                .Input(step => step.UserId, data => data.UserId)
                .Input(step => step.Message, _ => "Ожидайте одобрения заявки руководителем")
                .WaitFor("VacationReviewed", data => data.UserId, _ => DateTime.UtcNow)
                .Output(data => data.VacationReviewResult, step => step.EventData)
                .Output(data => data.IsVacationApproved, step => (string)step.EventData == "OK")

                .If(data => !data.IsVacationApproved).Do(__ => __
                    .StartWith<SendMessage>()
                    .Input(step => step.UserId, data => data.UserId)
                    .Input(step => step.Message, data => data.VacationReviewResult)))

            .Then<SendMessage>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message,
                data => $"Заявка на отпуск с '{data.StartDate}' на '{data.DaysAmount}' дней подтверждена")
            .EndWorkflow();
    }
}
