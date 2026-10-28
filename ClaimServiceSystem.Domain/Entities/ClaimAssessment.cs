namespace ClaimServiceSystem.Domain.Entities;

public sealed class ClaimAssessment
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public decimal AssessedAmount { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Guid RecordedByStaffId { get; set; }
    public StaffUser RecordedByStaff { get; set; } = null!;
    public DateTime RecordedAtUtc { get; set; }
}
