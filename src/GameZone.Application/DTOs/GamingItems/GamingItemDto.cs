namespace GameZone.Application.DTOs.GamingItems;

public class GamingItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RatePerHour { get; set; }
    public decimal Rate30MinOnePlayer { get; set; } = 80m;
    public decimal Rate30MinTwoPlayers { get; set; } = 120m;
    public decimal RateOneHourOnePlayer { get; set; } = 150m;
    public decimal RateOneHourTwoPlayers { get; set; } = 200m;
    public bool IsActive { get; set; }
    public string StatusText => IsActive ? "Active" : "Inactive";
    public string DisplayLabel => $"{Name}  ·  30m Rs {Rate30MinOnePlayer:N0}/{Rate30MinTwoPlayers:N0}  ·  1h Rs {RateOneHourOnePlayer:N0}/{RateOneHourTwoPlayers:N0}";
    public string PriceSummary => $"1P 30m Rs {Rate30MinOnePlayer:N0} / 1h Rs {RateOneHourOnePlayer:N0}   ·   2P 30m Rs {Rate30MinTwoPlayers:N0} / 1h Rs {RateOneHourTwoPlayers:N0}";

    public override string ToString() => DisplayLabel;
}
