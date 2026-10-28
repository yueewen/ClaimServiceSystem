namespace ClaimServiceSystem.Domain.Entities;

public sealed class InformationRequestResponse
{
    public Guid Id { get; set; }
    public Guid InformationRequestId { get; set; }
    public InformationRequest InformationRequest { get; set; } = null!;

    public Guid RespondedByClaimantId { get; set; }
    public Claimant RespondedByClaimant { get; set; } = null!;

    public string ResponseText { get; set; } = string.Empty;
    public DateTime RespondedAtUtc { get; set; }
}
