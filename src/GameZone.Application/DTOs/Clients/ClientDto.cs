namespace GameZone.Application.DTOs.Clients;

public class ClientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedDate { get; set; }
    public int TotalVisitCount { get; set; }
    public int FreeUseCount { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public string DisplayLabel => string.IsNullOrWhiteSpace(PhoneNumber) ? Name : $"{Name}  ({PhoneNumber})";

    public override string ToString() => DisplayLabel;
}
