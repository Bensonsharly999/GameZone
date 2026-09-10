using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Payments;
using GameZone.Application.Common;
using GameZone.Application.DTOs.Sessions;
using GameZone.Domain.Enums;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

[QueryProperty(nameof(SessionId), nameof(SessionId))]
public partial class EndSessionViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public EndSessionViewModel(GameZoneApi api)
    {
        _api = api;
    }

    [ObservableProperty] private int _sessionId;
    [ObservableProperty] private EndSessionResult? _result;
    [ObservableProperty] private PaymentMethod _method = PaymentMethod.Cash;
    [ObservableProperty] private string? _reference;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    public IReadOnlyList<PaymentMethod> Methods { get; } = PaymentRules.CheckoutMethods;

    [RelayCommand]
    private async Task EndAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            Result = await _api.EndSessionAsync(SessionId);
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

    [RelayCommand]
    private Task CollectPaidAsync() => SavePaymentAsync(true);

    [RelayCommand]
    private Task CollectPendingAsync() => SavePaymentAsync(false);

    private async Task SavePaymentAsync(bool paid)
    {
        if (Result is null)
            return;

        Error = null;
        IsBusy = true;
        try
        {
            await _api.RecordPaymentAsync(new RecordPaymentRequest
            {
                SessionId = Result.Session.Id,
                Amount = Result.Amount,
                PaymentMethod = Method,
                TransactionReference = Reference,
                PaymentStatus = paid ? PaymentStatus.Paid : PaymentStatus.Pending
            });
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
