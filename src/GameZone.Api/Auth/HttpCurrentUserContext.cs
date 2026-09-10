using GameZone.Application.DTOs.Auth;
using GameZone.Application.Interfaces;

namespace GameZone.Api.Auth;

public class HttpCurrentUserContext : ICurrentUserContext
{
    public CurrentUserDto? User { get; private set; }
    public bool IsAuthenticated => User is not null;
    public bool IsAdmin => User?.IsAdmin == true;

    public void SetUser(CurrentUserDto user) => User = user;
    public void Clear() => User = null;
}
