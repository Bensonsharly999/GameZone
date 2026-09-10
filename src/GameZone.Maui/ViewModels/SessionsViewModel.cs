using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Application.DTOs.Sessions;
using GameZone.Maui.Services;
using GameZone.Maui.Views;

namespace GameZone.Maui.ViewModels;

public partial class SessionsViewModel : ObservableObject
{
    private readonly GameZoneApi _api;

    public SessionsViewModel(GameZoneApi api)
    {
        _api = api;
    }

    public ObservableCollection<SessionDto> Sessions { get; } = new();

    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            var list = await _api.GetActiveSessionsAsync();
            Sessions.Clear();
            foreach (var item in list)
                Sessions.Add(item);
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
    private Task StartAsync() => Shell.Current.GoToAsync(nameof(StartSessionPage));

    [RelayCommand]
    private Task EndAsync(SessionDto? session)
        => session is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(EndSessionPage)}?SessionId={session.Id}");
}
