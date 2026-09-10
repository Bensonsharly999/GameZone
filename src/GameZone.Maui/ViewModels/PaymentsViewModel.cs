using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Payments;
using GameZone.Maui.Services;
using GameZone.Maui.Views;

namespace GameZone.Maui.ViewModels;

public partial class PaymentsViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public PaymentsViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<PaymentDto> Payments { get; } = new();

    [ObservableProperty] private bool _pendingOnly;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = PendingOnly
                ? await _api.GetPendingPaymentsAsync()
                : await _api.GetPaymentsAsync();
            Payments.Clear();
            foreach (var payment in list)
                Payments.Add(payment);
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
    private Task CollectAsync(PaymentDto? payment)
        => payment is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(PaymentFormPage)}?PaymentId={payment.Id}&SessionId={payment.SessionId}&AmountText={payment.Amount}");
}
