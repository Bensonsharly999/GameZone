namespace GameZone.Maui.Views;

public static class PageHelper
{
    public static T Bind<T>(this Page page) where T : class
    {
        if (page.BindingContext is T existing)
            return existing;

        var services = page.Handler?.MauiContext?.Services
                       ?? Application.Current?.Handler?.MauiContext?.Services
                       ?? throw new InvalidOperationException("Services are not ready.");
        var viewModel = services.GetRequiredService<T>();
        page.BindingContext = viewModel;
        return viewModel;
    }
}
