namespace ClaimServiceSystem.Application.DTOs;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public string Role { get; init; } = string.Empty;
}
