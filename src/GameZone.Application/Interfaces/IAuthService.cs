using GameZone.Application.Common;
using GameZone.Application.DTOs.Auth;

namespace GameZone.Application.Interfaces;

public interface IAuthService
{
    Task<Result<CurrentUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<string?> GetRoleHintAsync(string username, CancellationToken cancellationToken = default);
}
