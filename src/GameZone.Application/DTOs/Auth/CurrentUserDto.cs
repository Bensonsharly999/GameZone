using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Auth;

public class CurrentUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsAdmin => Role == UserRole.Admin;
}
