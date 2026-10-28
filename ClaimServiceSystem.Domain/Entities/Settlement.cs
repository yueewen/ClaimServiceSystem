namespace ClaimServiceSystem.Domain.Entities;

public sealed class Settlement
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public decimal Amount { get; set; }
    public SettlementState State { get; set; } = SettlementState.Created;

    public Guid CreatedByStaffId { get; set; }
    public StaffUser CreatedByStaff { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
