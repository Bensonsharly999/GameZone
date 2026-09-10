using GameZone.Maui.Services;
using GameZone.Maui.ViewModels;
using GameZone.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameZone.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddSingleton<AppSettings>();
        builder.Services.AddSingleton<SessionState>();
        builder.Services.AddSingleton<GameZoneApi>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<SessionsViewModel>();
        builder.Services.AddTransient<StartSessionViewModel>();
        builder.Services.AddTransient<EndSessionViewModel>();
        builder.Services.AddTransient<ClientsViewModel>();
        builder.Services.AddTransient<ClientFormViewModel>();
        builder.Services.AddTransient<ClientHistoryViewModel>();
        builder.Services.AddTransient<PaymentsViewModel>();
        builder.Services.AddTransient<PaymentFormViewModel>();
        builder.Services.AddTransient<ItemsViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<TodayViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<SessionsPage>();
        builder.Services.AddTransient<StartSessionPage>();
        builder.Services.AddTransient<EndSessionPage>();
        builder.Services.AddTransient<ClientsPage>();
        builder.Services.AddTransient<ClientFormPage>();
        builder.Services.AddTransient<ClientHistoryPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddTransient<PaymentFormPage>();
        builder.Services.AddTransient<ItemsPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TodayPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
