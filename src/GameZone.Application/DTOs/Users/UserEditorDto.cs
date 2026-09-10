using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Users;

public class UserEditorDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
}
