using System.ComponentModel.DataAnnotations;

namespace ClaimServiceSystem.Application.DTOs;

public sealed class LoginRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
