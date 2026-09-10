using GameZone.Application.Common;
using GameZone.Application.DTOs.Auth;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserContext _currentUser;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ICurrentUserContext currentUser)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<CurrentUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Result<CurrentUserDto>.Failure("Username and password are required.");

        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<CurrentUserDto>.Failure("Invalid username or password.");

        if (!user.IsActive)
            return Result<CurrentUserDto>.Failure("This account has been deactivated. Contact an administrator.");

        var dto = user.ToCurrentUser();
        _currentUser.SetUser(dto);
        return Result<CurrentUserDto>.Success(dto);
    }

    public async Task<string?> GetRoleHintAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var user = await _unitOfWork.Users.GetByUsernameAsync(username.Trim(), cancellationToken);
        return user is { IsActive: true } ? user.Role.ToString() : null;
    }
}
