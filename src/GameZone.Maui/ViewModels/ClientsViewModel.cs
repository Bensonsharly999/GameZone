using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Clients;
using GameZone.Maui.Services;
using GameZone.Maui.Views;

namespace GameZone.Maui.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public ClientsViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<ClientDto> Clients { get; } = new();

    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _api.GetClientsAsync(Search);
            Clients.Clear();
            foreach (var client in list)
                Clients.Add(client);
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
    private Task AddAsync() => Shell.Current.GoToAsync(nameof(ClientFormPage));

    [RelayCommand]
    private Task EditAsync(ClientDto? client)
        => client is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(ClientFormPage)}?ClientId={client.Id}");

    [RelayCommand]
    private Task HistoryAsync(ClientDto? client)
        => client is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(ClientHistoryPage)}?ClientId={client.Id}");

    [RelayCommand]
    private async Task DeleteAsync(ClientDto? client)
    {
        if (client is null) return;
        Error = null;
        try
        {
            await _api.DeleteClientAsync(client.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
