namespace ClaimServiceSystem.Domain.Entities;

public sealed class Claimant
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
