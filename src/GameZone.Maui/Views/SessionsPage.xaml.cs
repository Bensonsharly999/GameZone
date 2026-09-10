using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class SessionsPage : ContentPage
{
    public SessionsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<SessionsViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
