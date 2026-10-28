namespace ClaimServiceSystem.Domain.Entities;

public sealed class ClaimDecision
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public ClaimDecisionType Decision { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Guid DecidedByStaffId { get; set; }
    public StaffUser DecidedByStaff { get; set; } = null!;
    public DateTime DecidedAtUtc { get; set; }
}
