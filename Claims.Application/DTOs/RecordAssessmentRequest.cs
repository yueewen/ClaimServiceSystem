using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class RecordAssessmentRequest
{
    [Required]
    public string Notes { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal AssessedAmount { get; init; }
}
