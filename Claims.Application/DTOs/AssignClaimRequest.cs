using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class AssignClaimRequest
{
    [Required]
    public string OfficerId { get; init; } = string.Empty;
}
