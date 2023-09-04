using VacationBot.Steps;
using WorkflowCore.Interface;

namespace VacationBot.Workflows;

public class VacationWorkflow2 : IWorkflow<VacationWorkflowData>
{
    public string Id => "VacationWorkflow2";

    public int Version => 1;

    public void Build(IWorkflowBuilder<VacationWorkflowData> builder)
    {
        builder
            .SendMessage(data => data.UserId, "Привет! С помощью этого бота можно подать заявку на отпуск")
            .While(data => !data.IsVacationApproved).Do(_ => _
                .SendMessage(data => data.UserId, "Укажите дату начала отпуска")
                .WaitForUserMessage(data => data.UserId, data => data.StartDate)

                .SendMessage(data => data.UserId, "Укажите количество дней отпуска")
                .WaitForUserMessage(data => data.UserId, data => data.DaysAmount, x => int.Parse((string)x))

                .SendVariants(data => data.UserId, "Выберите тип отпуска",
                    new[] { "Основной", "Дополнительный", "Учебный", "Другой" })
                .WaitForUserMessage(data => data.UserId, data => data.VacationType)

                .If(data => data.DaysAmount > 14).Do(__ => __
                    .SendMessage(data => data.UserId, "Укажите вашего заместителя на время отпуска")
                    .WaitForUserMessage(data => data.UserId, data => data.Deputy))

                .SendMessage(data => data.UserId, "Ожидайте одобрения заявки руководителем")
                .WaitFor("VacationReviewed", data => data.UserId, _ => DateTime.UtcNow)
                .Output(data => data.VacationReviewResult, step => step.EventData)
                .Output(data => data.IsVacationApproved, step => (string)step.EventData == "OK")

                .If(data => !data.IsVacationApproved).Do(__ => __
                    .SendMessage(data => data.UserId, data => data.VacationReviewResult!)))

            .SendMessage(data => data.UserId,
                data => $"Заявка на отпуск с '{data.StartDate}' на '{data.DaysAmount}' дней подтверждена")
            .EndWorkflow();
    }
}
