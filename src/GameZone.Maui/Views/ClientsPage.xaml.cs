using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class ClientsPage : ContentPage
{
    public ClientsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<ClientsViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }

    private void OnSearchCompleted(object? sender, EventArgs e)
    {
        if (BindingContext is ClientsViewModel vm && vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
