using ClaimServiceSystem.Application.DTOs;

namespace ClaimServiceSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> Login(LoginRequest request);
}
