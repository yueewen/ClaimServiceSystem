using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class RequestAdditionalInformationRequest
{
    [Required]
    public string Message { get; init; } = string.Empty;
}
