namespace GameZone.Application.Common;

public static class CafeClock
{
    public static readonly TimeZoneInfo India = ResolveIndia();

    public static DateTime UtcNow => DateTime.UtcNow;

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, India);

    public static DateTime Today => Now.Date;

    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public static TimeSpan Played(DateTime entry)
    {
        var elapsed = UtcNow - ToUtc(entry);
        return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
    }

    public static (DateTime StartUtc, DateTime EndUtc) UtcDayRangeIst(DateTime? istDate = null)
    {
        var day = DateTime.SpecifyKind((istDate ?? Today).Date, DateTimeKind.Unspecified);
        var start = TimeZoneInfo.ConvertTimeToUtc(day, India);
        return (start, start.AddDays(1));
    }

    public static (DateTime StartUtc, DateTime EndUtc) UtcMonthRangeIst(DateTime? istDate = null)
    {
        var day = (istDate ?? Today).Date;
        var monthStartIst = DateTime.SpecifyKind(new DateTime(day.Year, day.Month, 1), DateTimeKind.Unspecified);
        var start = TimeZoneInfo.ConvertTimeToUtc(monthStartIst, India);
        var nextMonthIst = DateTime.SpecifyKind(monthStartIst.AddMonths(1), DateTimeKind.Unspecified);
        var end = TimeZoneInfo.ConvertTimeToUtc(nextMonthIst, India);
        return (start, end);
    }

    public static string ToIstOffsetString(DateTime value)
    {
        var ist = TimeZoneInfo.ConvertTimeFromUtc(ToUtc(value), India);
        return ist.ToString("yyyy-MM-dd'T'HH:mm:ss") + "+05:30";
    }

    private static TimeZoneInfo ResolveIndia()
    {
        foreach (var id in new[] { "India Standard Time", "Asia/Kolkata" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "India Standard Time");
    }
}
