namespace ClaimServiceSystem.Domain.Entities;

public sealed class ClaimStatusHistory
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public ClaimStatus FromStatus { get; set; }
    public ClaimStatus ToStatus { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
    public string? Reason { get; set; }
}
