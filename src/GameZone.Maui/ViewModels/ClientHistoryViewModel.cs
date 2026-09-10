using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Clients;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

[QueryProperty(nameof(ClientId), nameof(ClientId))]
public partial class ClientHistoryViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public ClientHistoryViewModel(GameZoneApi api)
    {
        _api = api;
    }

    [ObservableProperty] private int _clientId;
    [ObservableProperty] private ClientHistoryDto? _history;
    [ObservableProperty] private string? _error;

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            History = await _api.GetClientHistoryAsync(ClientId);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
