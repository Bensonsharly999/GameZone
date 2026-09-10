using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.Bind<SettingsViewModel>();
    }
}
