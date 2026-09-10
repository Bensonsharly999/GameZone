using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class ItemsPage : ContentPage
{
    public ItemsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<ItemsViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
