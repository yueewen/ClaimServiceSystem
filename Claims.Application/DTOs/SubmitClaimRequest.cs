using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class SubmitClaimRequest
{
    [Required]
    public string ClaimantId { get; init; } = string.Empty;

    [Required]
    public string PolicyNumber { get; init; } = string.Empty;

    [Required]
    public string ClaimType { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }
}
