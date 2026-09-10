using GameZone.Api.Auth;
using GameZone.Api.Contracts;
using GameZone.Application.DTOs.Auth;
using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtTokenService _tokens;

    public AuthController(IAuthService authService, JwtTokenService tokens)
    {
        _authService = authService;
        _tokens = tokens;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
            return Unauthorized(new { error = result.Error ?? "Invalid username or password." });

        return Ok(new LoginResponse
        {
            Token = _tokens.CreateToken(result.Value),
            User = result.Value
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUserContext currentUser)
        => currentUser.User is null ? Unauthorized() : Ok(currentUser.User);
}
