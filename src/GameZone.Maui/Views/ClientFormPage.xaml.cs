using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class ClientFormPage : ContentPage
{
    public ClientFormPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<ClientFormViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
