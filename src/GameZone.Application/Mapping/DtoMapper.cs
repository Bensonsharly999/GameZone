using GameZone.Application.DTOs.Auth;
using GameZone.Application.DTOs.Clients;
using GameZone.Application.DTOs.GamingItems;
using GameZone.Application.DTOs.Payments;
using GameZone.Application.DTOs.Sessions;
using GameZone.Application.DTOs.Users;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;

namespace GameZone.Application.Mapping;

public static class DtoMapper
{
    public static CurrentUserDto ToCurrentUser(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Username = user.Username,
        Role = user.Role
    };

    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Username = user.Username,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedDate = user.CreatedDate
    };

    public static ClientDto ToDto(this Client client, IEnumerable<Session>? sessions = null)
    {
        var list = (sessions ?? client.Sessions ?? Enumerable.Empty<Session>()).ToList();
        var completed = list.Where(s => s.SessionStatus == SessionStatus.Completed).ToList();

        return new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            Address = client.Address,
            CreatedDate = client.CreatedDate,
            TotalVisitCount = list.Count,
            FreeUseCount = list.Count(s =>
                (s.Payments ?? Enumerable.Empty<Payment>()).Any(p => p.PaymentMethod == PaymentMethod.Free)),
            TotalAmountSpent = completed.Sum(s => s.Amount ?? 0),
            LastVisitDate = list.OrderByDescending(s => s.EntryTime).FirstOrDefault()?.EntryTime
        };
    }

    public static GamingItemDto ToDto(this GamingItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        RatePerHour = item.RateOneHourOnePlayer > 0 ? item.RateOneHourOnePlayer : item.RatePerHour,
        Rate30MinOnePlayer = item.Rate30MinOnePlayer,
        Rate30MinTwoPlayers = item.Rate30MinTwoPlayers,
        RateOneHourOnePlayer = item.RateOneHourOnePlayer,
        RateOneHourTwoPlayers = item.RateOneHourTwoPlayers,
        IsActive = item.IsActive
    };

    public static SessionDto ToDto(this Session session)
    {
        var payment = session.Payments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
        return new SessionDto
        {
            Id = session.Id,
            ClientId = session.ClientId,
            ClientName = session.Client?.Name ?? string.Empty,
            PhoneNumber = session.Client?.PhoneNumber ?? string.Empty,
            GamingItemId = session.GamingItemId,
            GamingItemName = session.GamingItem?.Name ?? string.Empty,
            RatePerHour = HourlyRateFor(session),
            EntryTime = session.EntryTime,
            ExitTime = session.ExitTime,
            DurationMinutes = session.DurationMinutes,
            Amount = session.Amount,
            SessionStatus = session.SessionStatus,
            Notes = session.Notes,
            PlayerCount = session.PlayerCount < 1 ? 1 : session.PlayerCount,
            PaymentStatus = payment?.PaymentStatus.ToString() ?? "None",
            PaymentMethod = payment is null ? "-" : FormatMethod(payment.PaymentMethod)
        };
    }

    public static PaymentDto ToDto(this Payment payment) => new()
    {
        Id = payment.Id,
        SessionId = payment.SessionId,
        ClientName = payment.Session?.Client?.Name ?? string.Empty,
        GamingItemName = payment.Session?.GamingItem?.Name ?? string.Empty,
        Amount = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        TransactionReference = payment.TransactionReference,
        PaymentStatus = payment.PaymentStatus,
        PaymentDate = payment.PaymentDate,
        ReceivedBy = payment.ReceivedByUser?.Name ?? "-"
    };

    private static decimal HourlyRateFor(Session session)
    {
        var item = session.GamingItem;
        if (item is null) return 0;
        var twoPlus = session.PlayerCount >= 2;
        var rate = twoPlus ? item.RateOneHourTwoPlayers : item.RateOneHourOnePlayer;
        return rate > 0 ? rate : item.RatePerHour;
    }

    public static string FormatMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Upi => "UPI",
        PaymentMethod.CreditCard => "Credit Card",
        PaymentMethod.DebitCard => "Debit Card",
        PaymentMethod.Free => "Free",
        PaymentMethod.Discount => "Discount (30 min)",
        _ => method.ToString()
    };
}
