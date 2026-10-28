namespace ClaimServiceSystem.Application.DTOs;

public sealed class ClaimSummary
{
    public Guid Id { get; init; }
    public string ClaimantId { get; init; } = string.Empty;
    public string PolicyNumber { get; init; } = string.Empty;
    public string ClaimType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime SubmittedAtUtc { get; init; }
    public string Status { get; init; } = string.Empty;
}
