using GameZone.Domain.Entities;

namespace GameZone.Application.Common;

public static class SessionBilling
{
    public const int SlotMinutes = 30;
    public const int PaymentGraceMinutes = 5;

    public static int ToBilledMinutes(TimeSpan duration)
    {
        var actualMinutes = Math.Max(0, (int)Math.Ceiling(duration.TotalMinutes));
        var chargeableMinutes = Math.Max(1, actualMinutes - PaymentGraceMinutes);
        var slots = (int)Math.Ceiling(chargeableMinutes / (double)SlotMinutes);
        return Math.Max(SlotMinutes, slots * SlotMinutes);
    }

    public static decimal HalfHourRate(GamingItem item, int playerCount)
    {
        var twoPlus = playerCount >= 2;
        var hourRate = twoPlus ? item.RateOneHourTwoPlayers : item.RateOneHourOnePlayer;
        var halfRate = twoPlus ? item.Rate30MinTwoPlayers : item.Rate30MinOnePlayer;

        if (hourRate <= 0 && item.RatePerHour > 0)
            hourRate = item.RatePerHour;
        if (halfRate <= 0 && hourRate > 0)
            halfRate = Math.Round(hourRate / 2m, 2);

        return halfRate;
    }

    public static decimal CalculateAmount(int billedMinutes, GamingItem item, int playerCount)
    {
        var hourRate = HourRate(item, playerCount);
        var halfRate = HalfHourRate(item, playerCount);
        var fullHours = billedMinutes / 60;
        var extraHalfHour = billedMinutes % 60 >= SlotMinutes;
        return Math.Round((fullHours * hourRate) + (extraHalfHour ? halfRate : 0), 2, MidpointRounding.AwayFromZero);
    }

    private static decimal HourRate(GamingItem item, int playerCount)
    {
        var twoPlus = playerCount >= 2;
        var hourRate = twoPlus ? item.RateOneHourTwoPlayers : item.RateOneHourOnePlayer;
        if (hourRate <= 0 && item.RatePerHour > 0)
            hourRate = item.RatePerHour;
        return hourRate;
    }
}
