using GameZone.Application.Common;
using GameZone.Application.DTOs.Sessions;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SessionDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _unitOfWork.Sessions.GetActiveSessionsAsync(cancellationToken);
        var dtos = new List<SessionDto>();
        foreach (var session in sessions)
            dtos.Add(await ToDtoWithVisitsAsync(session, cancellationToken));
        return dtos;
    }

    public async Task<IReadOnlyList<SessionDto>> GetTodaysAsync(CancellationToken cancellationToken = default)
    {
        var (start, end) = CafeClock.UtcDayRangeIst();
        var sessions = await _unitOfWork.Sessions.GetByDateRangeAsync(start, end, cancellationToken);
        return sessions
            .OrderByDescending(s => s.EntryTime)
            .Select(s => s.ToDto())
            .ToList();
    }

    public async Task<Result<SessionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Sessions.GetWithDetailsAsync(id, cancellationToken);
        return session is null
            ? Result<SessionDto>.Failure("Session not found.")
            : Result<SessionDto>.Success(await ToDtoWithVisitsAsync(session, cancellationToken));
    }

    public async Task<Result<SessionDto>> StartAsync(StartSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ClientId <= 0)
            return Result<SessionDto>.Failure("Select a client.");
        if (request.GamingItemId <= 0)
            return Result<SessionDto>.Failure("Select a gaming item.");
        if (request.PlayerCount < 1 || request.PlayerCount > 8)
            return Result<SessionDto>.Failure("Select how many players are playing (1 to 8).");

        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result<SessionDto>.Failure("Client not found.");

        var item = await _unitOfWork.GamingItems.GetByIdAsync(request.GamingItemId, cancellationToken);
        if (item is null || !item.IsActive)
            return Result<SessionDto>.Failure("Selected gaming item is not available.");

        var alreadyActive = await _unitOfWork.Sessions.AnyAsync(
            s => s.ClientId == request.ClientId && s.SessionStatus == SessionStatus.Active,
            cancellationToken);
        if (alreadyActive)
            return Result<SessionDto>.Failure("This client already has an active session.");

        var itemBusy = await _unitOfWork.Sessions.AnyAsync(
            s => s.GamingItemId == request.GamingItemId && s.SessionStatus == SessionStatus.Active,
            cancellationToken);
        if (itemBusy)
            return Result<SessionDto>.Failure("This gaming item is already in an active session.");

        var session = new Session
        {
            ClientId = request.ClientId,
            GamingItemId = request.GamingItemId,
            EntryTime = CafeClock.UtcNow,
            SessionStatus = SessionStatus.Active,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            PlayerCount = request.PlayerCount
        };

        await _unitOfWork.Sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Sessions.GetWithDetailsAsync(session.Id, cancellationToken);
        return Result<SessionDto>.Success((created ?? session).ToDto());
    }

    public async Task<Result<EndSessionResult>> EndAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Sessions.GetWithDetailsAsync(sessionId, cancellationToken);
        if (session is null)
            return Result<EndSessionResult>.Failure("Session not found.");
        if (session.SessionStatus != SessionStatus.Active)
            return Result<EndSessionResult>.Success(await ToEndResultAsync(session, cancellationToken));

        var exitTime = CafeClock.UtcNow;
        if (exitTime < CafeClock.ToUtc(session.EntryTime))
            exitTime = CafeClock.ToUtc(session.EntryTime);

        var durationMinutes = SessionBilling.ToBilledMinutes(CafeClock.Played(session.EntryTime));
        var rate = session.PlayerCount >= 2
            ? session.GamingItem.RateOneHourTwoPlayers
            : session.GamingItem.RateOneHourOnePlayer;
        var amount = SessionBilling.CalculateAmount(durationMinutes, session.GamingItem, session.PlayerCount);

        session.ExitTime = exitTime;
        session.DurationMinutes = durationMinutes;
        session.Amount = amount;
        session.SessionStatus = SessionStatus.Completed;

        _unitOfWork.Sessions.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EndSessionResult>.Success(await ToEndResultAsync(session, cancellationToken));
    }

    private async Task<SessionDto> ToDtoWithVisitsAsync(Session session, CancellationToken cancellationToken)
    {
        var dto = session.ToDto();
        var visits = await _unitOfWork.Sessions.GetByClientAsync(session.ClientId, cancellationToken);
        dto.VisitNumber = visits.Count(s => s.EntryTime <= session.EntryTime);
        if (dto.VisitNumber <= 0)
            dto.VisitNumber = visits.Count;
        dto.FreeEligible = PaymentRules.IsLoyaltyFreeVisit(dto.VisitNumber);
        if (session.SessionStatus == SessionStatus.Active && session.GamingItem is not null)
        {
            var billed = SessionBilling.ToBilledMinutes(CafeClock.Played(session.EntryTime));
            dto.DurationMinutes = billed;
            dto.Amount = SessionBilling.CalculateAmount(billed, session.GamingItem, session.PlayerCount);
        }
        return dto;
    }

    private async Task<EndSessionResult> ToEndResultAsync(Session session, CancellationToken cancellationToken)
    {
        var billed = session.DurationMinutes
            ?? SessionBilling.ToBilledMinutes(CafeClock.Played(session.EntryTime));
        decimal amount;
        decimal half;
        decimal rate;
        if (session.GamingItem is not null)
        {
            amount = session.Amount
                ?? SessionBilling.CalculateAmount(billed, session.GamingItem, session.PlayerCount);
            half = SessionBilling.HalfHourRate(session.GamingItem, session.PlayerCount);
            rate = session.PlayerCount >= 2
                ? session.GamingItem.RateOneHourTwoPlayers
                : session.GamingItem.RateOneHourOnePlayer;
        }
        else
        {
            amount = session.Amount ?? 0;
            half = 0;
            rate = 0;
        }

        return new EndSessionResult
        {
            Session = await ToDtoWithVisitsAsync(session, cancellationToken),
            Duration = TimeSpan.FromMinutes(billed),
            RatePerHour = rate,
            Amount = amount,
            HalfHourAmount = half
        };
    }
}
