namespace ClaimServiceSystem.Domain.Entities;

public sealed class Claim
{
    public Guid Id { get; set; }
    public Guid ClaimantEntityId { get; set; }
    public Claimant Claimant { get; set; } = null!;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

    public Guid? AssignedOfficerEntityId { get; set; }
    public StaffUser? AssignedOfficer { get; set; }

    public ICollection<ClaimAssessment> Assessments { get; set; } = new List<ClaimAssessment>();
    public ClaimDecision? ClaimDecision { get; set; }
    public ICollection<InformationRequest> InformationRequests { get; set; } = new List<InformationRequest>();
    public Settlement? Settlement { get; set; }
    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}
