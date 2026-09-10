using GameZone.Maui.Views;

namespace GameZone.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(StartSessionPage), typeof(StartSessionPage));
        Routing.RegisterRoute(nameof(EndSessionPage), typeof(EndSessionPage));
        Routing.RegisterRoute(nameof(ClientFormPage), typeof(ClientFormPage));
        Routing.RegisterRoute(nameof(ClientHistoryPage), typeof(ClientHistoryPage));
        Routing.RegisterRoute(nameof(PaymentFormPage), typeof(PaymentFormPage));
    }
}
