using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Payments;

public class RecordPaymentRequest
{
    public int SessionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    public int DiscountPercent { get; set; }
}
