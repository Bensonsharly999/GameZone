using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class StartSessionPage : ContentPage
{
    public StartSessionPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<StartSessionViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }

    private void OnSearchCompleted(object? sender, EventArgs e)
    {
        if (BindingContext is StartSessionViewModel vm && vm.SearchClientsCommand.CanExecute(null))
            vm.SearchClientsCommand.Execute(null);
    }
}
