namespace GameZone.Domain.Entities;

public class GamingItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RatePerHour { get; set; }
    public decimal Rate30MinOnePlayer { get; set; } = 80m;
    public decimal Rate30MinTwoPlayers { get; set; } = 120m;
    public decimal RateOneHourOnePlayer { get; set; } = 150m;
    public decimal RateOneHourTwoPlayers { get; set; } = 200m;
    public bool IsActive { get; set; } = true;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
