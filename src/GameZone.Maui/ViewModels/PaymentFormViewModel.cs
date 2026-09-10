using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.Common;
using GameZone.Application.DTOs.Payments;
using GameZone.Domain.Enums;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

[QueryProperty(nameof(PaymentId), nameof(PaymentId))]
[QueryProperty(nameof(SessionId), nameof(SessionId))]
[QueryProperty(nameof(AmountText), nameof(AmountText))]
public partial class PaymentFormViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public PaymentFormViewModel(GameZoneApi api)
    {
        _api = api;
    }

    [ObservableProperty] private int _paymentId;
    [ObservableProperty] private int _sessionId;
    [ObservableProperty] private string _amountText = string.Empty;
    [ObservableProperty] private PaymentMethod _method = PaymentMethod.Cash;
    [ObservableProperty] private string? _reference;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    public IReadOnlyList<PaymentMethod> Methods { get; } = PaymentRules.CheckoutMethods;

    [RelayCommand]
    private async Task SaveAsync()
    {
        decimal amount = 0;
        if (Method != PaymentMethod.Free)
        {
            if (!decimal.TryParse(AmountText, out amount) || amount <= 0)
            {
                Error = "Enter a valid amount.";
                return;
            }
        }

        Error = null;
        IsBusy = true;
        try
        {
            var request = new RecordPaymentRequest
            {
                SessionId = SessionId,
                Amount = amount,
                PaymentMethod = Method,
                TransactionReference = Reference,
                PaymentStatus = PaymentStatus.Paid
            };
            if (PaymentId > 0)
                await _api.MarkPaidAsync(PaymentId, request);
            else
                await _api.RecordPaymentAsync(request);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
