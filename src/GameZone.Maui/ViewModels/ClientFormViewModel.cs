using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Clients;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

[QueryProperty(nameof(ClientId), nameof(ClientId))]
public partial class ClientFormViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public ClientFormViewModel(GameZoneApi api)
    {
        _api = api;
    }

    [ObservableProperty] private int _clientId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;
    public string Title => ClientId > 0 ? "Edit Client" : "Add Client";

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (ClientId <= 0)
            return;

        try
        {
            var clients = await _api.GetClientsAsync();
            var client = clients.FirstOrDefault(c => c.Id == ClientId);
            if (client is null)
                return;
            Name = client.Name;
            Phone = client.PhoneNumber;
            Address = client.Address;
            OnPropertyChanged(nameof(Title));
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var dto = new ClientEditorDto
            {
                Id = ClientId > 0 ? ClientId : null,
                Name = Name,
                PhoneNumber = Phone,
                Address = Address
            };
            if (ClientId > 0)
                await _api.UpdateClientAsync(ClientId, dto);
            else
                await _api.CreateClientAsync(dto);
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
