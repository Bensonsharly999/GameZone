using System.Security.Claims;
using GameZone.Application.DTOs.Auth;
using GameZone.Application.Interfaces;
using GameZone.Domain.Enums;

namespace GameZone.Api.Auth;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserContext currentUser)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleValue = context.User.FindFirstValue(ClaimTypes.Role);
            if (int.TryParse(idValue, out var id) && Enum.TryParse<UserRole>(roleValue, out var role))
            {
                currentUser.SetUser(new CurrentUserDto
                {
                    Id = id,
                    Name = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                    Username = context.User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                    Role = role
                });
            }
        }

        await _next(context);
    }
}
