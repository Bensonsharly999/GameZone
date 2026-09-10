using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Clients;
using GameZone.Application.DTOs.GamingItems;
using GameZone.Application.DTOs.Sessions;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class StartSessionViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public StartSessionViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<ClientDto> Clients { get; } = new();
    public ObservableCollection<GamingItemDto> Items { get; } = new();

    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private ClientDto? _selectedClient;
    [ObservableProperty] private GamingItemDto? _selectedItem;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _playerCount = 1;

    public IReadOnlyList<int> PlayerCounts { get; } = Enumerable.Range(1, 8).ToList();

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var items = await _api.GetActiveItemsAsync();
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            SelectedItem ??= Items.FirstOrDefault();
            await SearchClientsAsync();
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
    private async Task SearchClientsAsync()
    {
        try
        {
            var clients = await _api.GetClientsAsync(Search);
            var previousId = SelectedClient?.Id;
            Clients.Clear();
            foreach (var client in clients)
                Clients.Add(client);
            SelectedClient = Clients.FirstOrDefault(c => c.Id == previousId) ?? Clients.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (SelectedClient is null || SelectedItem is null)
        {
            Error = "Select a client and a gaming item.";
            return;
        }

        Error = null;
        IsBusy = true;
        try
        {
            await _api.StartSessionAsync(new StartSessionRequest
            {
                ClientId = SelectedClient.Id,
                GamingItemId = SelectedItem.Id,
                PlayerCount = PlayerCount,
                Notes = Notes
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
