namespace ClaimServiceSystem.Application.DTOs;

public sealed class InformationRequestView
{
    public Guid RequestId { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime RequestedAtUtc { get; init; }
    public DateTime? RespondedAtUtc { get; init; }
    public string? Response { get; init; }
}
