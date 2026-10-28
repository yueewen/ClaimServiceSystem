namespace ClaimServiceSystem.Application.DTOs;

public sealed class SettlementResponse
{
    public Guid ClaimId { get; init; }
    public decimal Amount { get; init; }
    public string State { get; init; } = string.Empty;
    public DateTime? CreatedAtUtc { get; init; }
    public DateTime? SettledAtUtc { get; init; }
}
