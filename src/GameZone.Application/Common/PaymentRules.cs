using GameZone.Domain.Enums;

namespace GameZone.Application.Common;

public sealed record CheckoutTotals(decimal Payable, decimal Discount);

public static class PaymentRules
{
    public const int LoyaltyVisitInterval = 5;

    public static readonly IReadOnlyList<int> DiscountPercents =
        [5, 10, 15, 20, 25, 30, 40, 50, 75, 100];

    public static readonly IReadOnlyList<PaymentMethod> CheckoutMethods =
    [
        PaymentMethod.Cash,
        PaymentMethod.Upi,
        PaymentMethod.Free,
        PaymentMethod.Discount
    ];

    public static bool IsLoyaltyFreeVisit(int visitNumber)
        => visitNumber > 0 && visitNumber % LoyaltyVisitInterval == 0;

    public static IReadOnlyList<PaymentMethod> MethodsFor(bool freeEligible)
        => CheckoutMethods;

    public static Result<CheckoutTotals> Resolve(
        PaymentMethod method,
        decimal fullAmount,
        decimal halfHourAmount,
        int visitNumber,
        int discountPercent)
    {
        fullAmount = Math.Max(0, Math.Round(fullAmount, 2, MidpointRounding.AwayFromZero));
        halfHourAmount = Math.Max(0, Math.Round(halfHourAmount, 2, MidpointRounding.AwayFromZero));
        discountPercent = Math.Clamp(discountPercent, 0, 100);

        if (method is PaymentMethod.CreditCard or PaymentMethod.DebitCard or PaymentMethod.Wallet)
            return Result<CheckoutTotals>.Failure("This payment method is not available.");

        if (method is PaymentMethod.Cash or PaymentMethod.Upi)
            return Result<CheckoutTotals>.Success(new CheckoutTotals(fullAmount, 0));

        if (method == PaymentMethod.Free)
            return Result<CheckoutTotals>.Success(new CheckoutTotals(0, fullAmount));

        if (method == PaymentMethod.Discount)
        {
            var discount = Math.Round(fullAmount * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);
            var payable = Math.Max(0, fullAmount - discount);
            return Result<CheckoutTotals>.Success(new CheckoutTotals(payable, discount));
        }

        return Result<CheckoutTotals>.Failure("Choose Cash, UPI, Free, or Discount.");
    }
}
