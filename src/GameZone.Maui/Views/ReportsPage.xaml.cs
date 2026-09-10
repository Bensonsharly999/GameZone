using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class ReportsPage : ContentPage
{
    public ReportsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<ReportsViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
