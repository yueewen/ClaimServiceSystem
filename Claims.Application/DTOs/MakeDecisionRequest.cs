using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class MakeDecisionRequest
{
    [Required]
    [RegularExpression("Approved|Rejected")]
    public string Decision { get; init; } = string.Empty;

    [Required]
    public string Reason { get; init; } = string.Empty;
}
