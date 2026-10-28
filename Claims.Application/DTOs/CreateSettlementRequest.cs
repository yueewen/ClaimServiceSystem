using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class CreateSettlementRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }
}
