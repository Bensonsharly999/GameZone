using GameZone.Application.Common;
using GameZone.Application.DTOs.GamingItems;

namespace GameZone.Application.Interfaces;

public interface IGamingItemService
{
    Task<IReadOnlyList<GamingItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GamingItemDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<GamingItemDto>> CreateAsync(GamingItemDto dto, CancellationToken cancellationToken = default);
    Task<Result<GamingItemDto>> UpdateAsync(GamingItemDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
