using GameZone.Application.Interfaces;
using GameZone.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameZone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IClientService, ClientService>();
        services.AddTransient<IGamingItemService, GamingItemService>();
        services.AddTransient<ISessionService, SessionService>();
        services.AddTransient<IPaymentService, PaymentService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.AddTransient<IReportService, ReportService>();
        return services;
    }
}
