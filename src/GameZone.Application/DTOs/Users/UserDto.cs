using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public string RoleText => Role.ToString();
    public string StatusText => IsActive ? "Active" : "Inactive";
}
