using GameZone.Application.DTOs.Auth;

namespace GameZone.Api.Contracts;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public CurrentUserDto User { get; set; } = new();
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
