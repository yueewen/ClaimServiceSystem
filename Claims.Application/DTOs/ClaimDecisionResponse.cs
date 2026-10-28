namespace ClaimServiceSystem.Application.DTOs;

public sealed class ClaimDecisionResponse
{
    public Guid ClaimId { get; init; }
    public string Decision { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime? DecidedAtUtc { get; init; }
}
