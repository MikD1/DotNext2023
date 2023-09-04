namespace VacationBot.Workflows;

public class VacationWorkflowData
{
    public string UserId { get; set; } = default!;

    public bool IsVacationApproved { get; set; }

    public string? StartDate { get; set; }

    public string? VacationType { get; set; }

    public int DaysAmount { get; set; }

    public string? Deputy { get; set; }

    public string? VacationReviewResult { get; set; }
}