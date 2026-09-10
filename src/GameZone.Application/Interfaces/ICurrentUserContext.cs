using GameZone.Application.DTOs.Auth;

namespace GameZone.Application.Interfaces;

public interface ICurrentUserContext
{
    CurrentUserDto? User { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    void SetUser(CurrentUserDto user);
    void Clear();
}
