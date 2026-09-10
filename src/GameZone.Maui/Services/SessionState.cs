using GameZone.Application.DTOs.Auth;

namespace GameZone.Maui.Services;

public class SessionState
{
    public string? Token { get; set; }
    public CurrentUserDto? User { get; set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && User is not null;
    public bool IsAdmin => User?.IsAdmin == true;

    public void SignIn(string token, CurrentUserDto user)
    {
        Token = token;
        User = user;
    }

    public void SignOut()
    {
        Token = null;
        User = null;
    }
}
