using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Sessions;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class TodayViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public TodayViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<SessionDto> Sessions { get; } = new();
    [ObservableProperty] private string? _error;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        try
        {
            var list = await _api.GetTodaysSessionsAsync();
            Sessions.Clear();
            foreach (var session in list)
                Sessions.Add(session);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
