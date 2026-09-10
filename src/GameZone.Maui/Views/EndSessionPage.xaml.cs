using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class EndSessionPage : ContentPage
{
    public EndSessionPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.Bind<EndSessionViewModel>();
    }
}
