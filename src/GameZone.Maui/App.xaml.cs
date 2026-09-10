using GameZone.Maui.Services;
using GameZone.Maui.Views;

namespace GameZone.Maui;

public partial class App : Application
{
    public App(LoginPage loginPage)
    {
        InitializeComponent();
        MainPage = loginPage;
    }

    public void ShowShell()
    {
        MainPage = Handler?.MauiContext?.Services.GetRequiredService<AppShell>()
                   ?? throw new InvalidOperationException("AppShell is not registered.");
    }

    public void ShowLogin()
    {
        var settings = Handler?.MauiContext?.Services.GetRequiredService<SessionState>();
        settings?.SignOut();
        MainPage = Handler?.MauiContext?.Services.GetRequiredService<LoginPage>();
    }
}
