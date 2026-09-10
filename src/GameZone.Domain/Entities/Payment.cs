using GameZone.Domain.Enums;

namespace GameZone.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public int? ReceivedByUserId { get; set; }

    public Session Session { get; set; } = null!;
    public User? ReceivedByUser { get; set; }
}
