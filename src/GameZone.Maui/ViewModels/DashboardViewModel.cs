using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly GameZoneApi _api;
    private readonly SessionState _session;

    public DashboardViewModel(GameZoneApi api, SessionState session)
    {
        _api = api;
        _session = session;
    }

    public string Greeting => $"Hi, {_session.User?.Name ?? "there"}";

    [ObservableProperty] private int _todaysClients;
    [ObservableProperty] private int _activeSessions;
    [ObservableProperty] private decimal _todaysRevenue;
    [ObservableProperty] private decimal _monthlyRevenue;
    [ObservableProperty] private string _monthLabel = string.Empty;
    [ObservableProperty] private int _pendingPayments;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    public bool IsAdmin => _session.IsAdmin;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var stats = await _api.GetDashboardAsync();
            TodaysClients = stats.TodaysClientsCount;
            ActiveSessions = stats.ActiveSessions;
            TodaysRevenue = stats.TodaysRevenue;
            MonthlyRevenue = stats.MonthlyRevenue;
            MonthLabel = stats.MonthLabel;
            PendingPayments = stats.PendingPayments;
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(IsAdmin));
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
