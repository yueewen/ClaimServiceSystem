using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class InformationRequestResponse
{
    [Required]
    public string Response { get; init; } = string.Empty;
}
