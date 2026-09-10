using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<UsersViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
