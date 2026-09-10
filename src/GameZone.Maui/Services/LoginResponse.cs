using GameZone.Application.DTOs.Auth;

namespace GameZone.Maui.Services;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public CurrentUserDto User { get; set; } = new();
}

public class ApiError
{
    public string? Error { get; set; }
}
