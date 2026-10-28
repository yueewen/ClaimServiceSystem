namespace ClaimServiceSystem.Domain.Entities;

public sealed class AuthUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
}
