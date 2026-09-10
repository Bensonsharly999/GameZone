using GameZone.Application.Common;
using GameZone.Application.DTOs.Payments;
using GameZone.Application.Interfaces;
using GameZone.Application.Mapping;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public PaymentService(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllWithDetailsAsync(cancellationToken);
        return payments.OrderByDescending(p => p.PaymentDate).Select(p => p.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetByStatusAsync(PaymentStatus.Pending, cancellationToken);
        return payments.OrderByDescending(p => p.PaymentDate).Select(p => p.ToDto()).ToList();
    }

    public async Task<Result<PaymentDto>> RecordAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Sessions.GetWithDetailsAsync(request.SessionId, cancellationToken);
        if (session is null)
            return Result<PaymentDto>.Failure("Session not found.");

        var existing = await _unitOfWork.Payments.GetBySessionIdAsync(request.SessionId, cancellationToken);
        if (existing is not null)
            return Result<PaymentDto>.Success(existing.ToDto());

        if (request.PaymentMethod == PaymentMethod.Free)
            return await SaveAsync(session, 0, PaymentMethod.Free, PaymentStatus.Paid, request.TransactionReference, cancellationToken);

        var billedMinutes = session.DurationMinutes
            ?? (session.SessionStatus == SessionStatus.Active
                ? SessionBilling.ToBilledMinutes(DateTime.Now - session.EntryTime)
                : (int?)null);
        if (session.GamingItem is null)
            return Result<PaymentDto>.Failure("Gaming item not found.");
        if (billedMinutes is null)
            return Result<PaymentDto>.Failure("Session is not billed yet.");

        var full = SessionBilling.CalculateAmount(billedMinutes.Value, session.GamingItem, session.PlayerCount);
        var half = SessionBilling.HalfHourRate(session.GamingItem, session.PlayerCount);
        if (full <= 0 && request.Amount > 0)
            full = request.Amount;
        var visits = await _unitOfWork.Sessions.GetByClientAsync(session.ClientId, cancellationToken);
        var visitNumber = visits.Count(s => s.EntryTime <= session.EntryTime);
        var resolved = PaymentRules.Resolve(request.PaymentMethod, full, half, visitNumber, request.DiscountPercent);
        if (!resolved.IsSuccess)
            return Result<PaymentDto>.Failure(resolved.Error ?? "Invalid payment.");

        var status = resolved.Value!.Payable <= 0
            ? PaymentStatus.Paid
            : request.PaymentStatus;
        return await SaveAsync(session, resolved.Value.Payable, request.PaymentMethod, status, request.TransactionReference, cancellationToken);
    }

    private async Task<Result<PaymentDto>> SaveAsync(
        Session session,
        decimal amount,
        PaymentMethod method,
        PaymentStatus status,
        string? reference,
        CancellationToken cancellationToken)
    {
        session.Amount = amount;
        _unitOfWork.Sessions.Update(session);

        var payment = new Payment
        {
            SessionId = session.Id,
            Amount = amount,
            PaymentMethod = method,
            TransactionReference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            PaymentStatus = status,
            PaymentDate = DateTime.Now,
            ReceivedByUserId = _currentUser.User?.Id
        };

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Payments.GetWithDetailsAsync(payment.Id, cancellationToken);
        return Result<PaymentDto>.Success((created ?? payment).ToDto());
    }

    public async Task<Result<PaymentDto>> MarkPaidAsync(int paymentId, RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetWithDetailsAsync(paymentId, cancellationToken);
        if (payment is null)
            return Result<PaymentDto>.Failure("Payment not found.");

        if (request.PaymentMethod == PaymentMethod.Free)
        {
            payment.Amount = 0;
            payment.PaymentMethod = PaymentMethod.Free;
            if (payment.Session is not null)
            {
                payment.Session.Amount = 0;
                _unitOfWork.Sessions.Update(payment.Session);
            }
            payment.TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference)
                ? payment.TransactionReference
                : request.TransactionReference.Trim();
            payment.PaymentStatus = PaymentStatus.Paid;
            payment.PaymentDate = DateTime.Now;
            payment.ReceivedByUserId = _currentUser.User?.Id;
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var freeUpdated = await _unitOfWork.Payments.GetWithDetailsAsync(payment.Id, cancellationToken);
            return Result<PaymentDto>.Success((freeUpdated ?? payment).ToDto());
        }

        var billedMinutes = payment.Session?.DurationMinutes
            ?? (payment.Session is { SessionStatus: SessionStatus.Active }
                ? SessionBilling.ToBilledMinutes(DateTime.Now - payment.Session.EntryTime)
                : payment.Session?.DurationMinutes);
        if (payment.Session?.GamingItem is null)
            return Result<PaymentDto>.Failure("Gaming item not found.");
        if (billedMinutes is null)
            return Result<PaymentDto>.Failure("Session is not billed yet.");

        var full = SessionBilling.CalculateAmount(billedMinutes.Value, payment.Session.GamingItem, payment.Session.PlayerCount);
        var half = SessionBilling.HalfHourRate(payment.Session.GamingItem, payment.Session.PlayerCount);
        var visits = await _unitOfWork.Sessions.GetByClientAsync(payment.Session.ClientId, cancellationToken);
        var visitNumber = visits.Count(s => s.EntryTime <= payment.Session.EntryTime);
        var resolved = PaymentRules.Resolve(request.PaymentMethod, full, half, visitNumber, request.DiscountPercent);
        if (!resolved.IsSuccess)
            return Result<PaymentDto>.Failure(resolved.Error ?? "Invalid payment.");

        payment.Amount = resolved.Value.Payable;
        payment.PaymentMethod = request.PaymentMethod;
        payment.Session.Amount = resolved.Value.Payable;
        _unitOfWork.Sessions.Update(payment.Session);
        payment.TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference)
            ? payment.TransactionReference
            : request.TransactionReference.Trim();
        payment.PaymentStatus = PaymentStatus.Paid;
        payment.PaymentDate = DateTime.Now;
        payment.ReceivedByUserId = _currentUser.User?.Id;

        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Payments.GetWithDetailsAsync(payment.Id, cancellationToken);
        return Result<PaymentDto>.Success((updated ?? payment).ToDto());
    }
}
