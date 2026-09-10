namespace GameZone.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
