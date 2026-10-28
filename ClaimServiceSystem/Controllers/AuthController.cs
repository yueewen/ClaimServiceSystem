using ClaimServiceSystem.Application.DTOs;
using ClaimServiceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimServiceSystem.Controllers;

[Route("api/v1/auth")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.Login(request);
        if (token is null)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(token);
    }
}
