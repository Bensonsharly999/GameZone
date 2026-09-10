namespace GameZone.Application.DTOs.Clients;

public class ClientEditorDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
}
