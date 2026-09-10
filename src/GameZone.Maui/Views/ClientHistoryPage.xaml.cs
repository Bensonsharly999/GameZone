using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class ClientHistoryPage : ContentPage
{
    public ClientHistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<ClientHistoryViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
