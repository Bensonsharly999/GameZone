using GameZone.Application.Common;
using GameZone.Application.DTOs.Users;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserContext _currentUser;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ICurrentUserContext currentUser)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.OrderBy(u => u.Role).ThenBy(u => u.Name).Select(u => u.ToDto()).ToList();
    }

    public async Task<Result<UserDto>> CreateAsync(UserEditorDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<UserDto>.Failure("Only administrators can manage users.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<UserDto>.Failure("Name is required.");
        if (string.IsNullOrWhiteSpace(dto.Username))
            return Result<UserDto>.Failure("Username is required.");
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return Result<UserDto>.Failure("Password must be at least 6 characters.");

        if (await _unitOfWork.Users.UsernameExistsAsync(dto.Username.Trim(), null, cancellationToken))
            return Result<UserDto>.Failure("Username is already taken.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Username = dto.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> UpdateAsync(UserEditorDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            return Result<UserDto>.Failure("Only administrators can manage users.");
        if (dto.Id is null)
            return Result<UserDto>.Failure("User id is required.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<UserDto>.Failure("Name is required.");
        if (string.IsNullOrWhiteSpace(dto.Username))
            return Result<UserDto>.Failure("Username is required.");

        var user = await _unitOfWork.Users.GetByIdAsync(dto.Id.Value, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure("User not found.");

        if (await _unitOfWork.Users.UsernameExistsAsync(dto.Username.Trim(), user.Id, cancellationToken))
            return Result<UserDto>.Failure("Username is already taken.");

        user.Name = dto.Name.Trim();
        user.Username = dto.Username.Trim();
        user.Role = dto.Role;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure("Only administrators can manage users.");
        if (_currentUser.User?.Id == id)
            return Result.Failure("You cannot deactivate your own account.");

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");
        if (user.Role == UserRole.Admin)
            return Result.Failure("Administrator accounts cannot be deactivated from here.");

        user.IsActive = false;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure("Only administrators can manage users.");

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        user.IsActive = true;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            return Result.Failure("Only administrators can reset passwords.");
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return Result.Failure("Password must be at least 6 characters.");

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
