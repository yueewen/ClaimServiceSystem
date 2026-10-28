namespace ClaimServiceSystem.Domain.Entities;

public sealed class StaffUser
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public StaffRole Role { get; set; } = StaffRole.Officer;

    public ICollection<Claim> AssignedClaims { get; set; } = new List<Claim>();
    public ICollection<ClaimAssessment> RecordedAssessments { get; set; } = new List<ClaimAssessment>();
    public ICollection<ClaimDecision> RecordedDecisions { get; set; } = new List<ClaimDecision>();
    public ICollection<InformationRequest> RequestedInformationItems { get; set; } = new List<InformationRequest>();
    public ICollection<Settlement> CreatedSettlements { get; set; } = new List<Settlement>();
}

public enum StaffRole
{
    Officer = 1,
    Manager = 2
}
