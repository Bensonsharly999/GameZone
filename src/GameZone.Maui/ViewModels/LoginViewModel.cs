using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameZone.Maui.Services;

namespace GameZone.Maui.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly GameZoneApi _api;
    private readonly SessionState _session;
    private readonly AppSettings _settings;

    public LoginViewModel(GameZoneApi api, SessionState session, AppSettings settings)
    {
        _api = api;
        _session = session;
        _settings = settings;
        ApiUrl = _settings.ApiBaseUrl;
    }

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _apiUrl = string.Empty;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        Error = null;
        IsBusy = true;
        try
        {
            _settings.ApiBaseUrl = ApiUrl;
            var result = await _api.LoginAsync(Username, Password);
            _session.SignIn(result.Token, result.User);
            if (Application.Current is App app)
                app.ShowShell();
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
