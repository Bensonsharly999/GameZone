using GameZone.Application.DTOs.Dashboard;

namespace GameZone.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
