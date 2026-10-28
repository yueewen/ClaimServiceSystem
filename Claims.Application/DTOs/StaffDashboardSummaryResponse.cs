namespace ClaimServiceSystem.Application.DTOs;

public sealed class StaffDashboardSummaryResponse
{
    public string StaffId { get; init; } = string.Empty;
    public int TotalClaims { get; init; }
    public int AssignedToMe { get; init; }
    public int UnderReview { get; init; }
    public int InformationRequested { get; init; }
    public int AssessmentCompleted { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
    public int SettlementInProgress { get; init; }
    public int DecisionsMadeByMe { get; init; }
}
