namespace ClaimServiceSystem.Domain.Entities;

public sealed class InformationRequest
{
    public Guid Id { get; set; }
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public Guid RequestedByStaffId { get; set; }
    public StaffUser RequestedByStaff { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public InformationRequestStatus Status { get; set; } = InformationRequestStatus.Open;
    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<InformationRequestResponse> Responses { get; set; } = new List<InformationRequestResponse>();
}

public enum InformationRequestStatus
{
    Open = 1,
    Responded = 2,
    Closed = 3
}
