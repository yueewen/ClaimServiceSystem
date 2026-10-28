using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using ClaimServiceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClaimServiceSystem.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly ClaimsDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(ClaimsDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var username = request.Username.Trim();
        var passwordHash = ComputeHash(request.Password);

        var user = await _dbContext.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username && x.PasswordHash == passwordHash);

        if (user is null)
        {
            return null;
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "ClaimServiceSystem";
        var audience = _configuration["Jwt:Audience"] ?? "ClaimServiceSystem.Client";
        var key = _configuration["Jwt:Key"] ?? "ClaimServiceSystem-Local-Dev-Secret-Key-Change-Me";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var value) ? value : 60;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.SubjectId),
            new(ClaimTypes.NameIdentifier, user.SubjectId),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            Role = user.Role
        };
    }

    public static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
