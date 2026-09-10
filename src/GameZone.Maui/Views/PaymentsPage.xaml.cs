using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class PaymentsPage : ContentPage
{
    public PaymentsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = this.Bind<PaymentsViewModel>();
        if (vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }

    private void OnFilterChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is PaymentsViewModel vm && vm.LoadCommand.CanExecute(null))
            vm.LoadCommand.Execute(null);
    }
}
