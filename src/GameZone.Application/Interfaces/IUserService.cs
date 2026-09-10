using GameZone.Application.Common;
using GameZone.Application.DTOs.Users;

namespace GameZone.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(UserEditorDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(UserEditorDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default);
}
