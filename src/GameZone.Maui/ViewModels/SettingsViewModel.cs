using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly SessionState _session;

    public SettingsViewModel(AppSettings settings, SessionState session)
    {
        _settings = settings;
        _session = session;
        ApiUrl = _settings.ApiBaseUrl;
        UserName = _session.User?.Name ?? "-";
    }

    [ObservableProperty] private string _apiUrl = string.Empty;
    [ObservableProperty] private string _userName = "-";
    [ObservableProperty] private string? _status;

    [RelayCommand]
    private void Save()
    {
        _settings.ApiBaseUrl = ApiUrl;
        Status = "API URL saved.";
    }

    [RelayCommand]
    private void Logout()
    {
        if (Application.Current is App app)
            app.ShowLogin();
    }
}
