using GameZone.Application.Common;
using GameZone.Application.DTOs.Payments;

namespace GameZone.Application.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentDto>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> RecordAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkPaidAsync(int paymentId, RecordPaymentRequest request, CancellationToken cancellationToken = default);
}
