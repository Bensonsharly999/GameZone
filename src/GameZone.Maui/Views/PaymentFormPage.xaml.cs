using GameZone.Maui.ViewModels;

namespace GameZone.Maui.Views;

public partial class PaymentFormPage : ContentPage
{
    public PaymentFormPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.Bind<PaymentFormViewModel>();
    }
}
