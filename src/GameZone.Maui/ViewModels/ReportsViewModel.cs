using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Reports;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public ReportsViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<DailyRevenueDto> Daily { get; } = new();
    public ObservableCollection<TopClientDto> TopClients { get; } = new();

    [ObservableProperty] private string? _error;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        try
        {
            var from = DateTime.Today.AddDays(-6);
            var to = DateTime.Today;
            var daily = await _api.GetDailyRevenueAsync(from, to);
            Daily.Clear();
            foreach (var row in daily)
                Daily.Add(row);

            var top = await _api.GetTopClientsAsync();
            TopClients.Clear();
            foreach (var row in top)
                TopClients.Add(row);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
