using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class TodayPage : ContentPage
{
    public TodayPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<TodayViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
