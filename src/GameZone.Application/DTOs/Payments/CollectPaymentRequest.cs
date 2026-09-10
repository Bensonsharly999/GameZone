using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Payments;

public class CollectPaymentRequest
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public int DiscountPercent { get; set; }
    public string? TransactionReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
}
