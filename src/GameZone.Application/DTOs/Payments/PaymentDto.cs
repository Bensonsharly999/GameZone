using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Payments;

public class PaymentDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string GamingItemName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReceivedBy { get; set; } = "-";

    public string MethodText => PaymentMethod switch
    {
        PaymentMethod.Upi => "UPI",
        PaymentMethod.CreditCard => "Credit Card",
        PaymentMethod.DebitCard => "Debit Card",
        PaymentMethod.Free => "Free",
        PaymentMethod.Discount => "Discount (30 min)",
        _ => PaymentMethod.ToString()
    };

    public string StatusText => PaymentStatus.ToString();
}
