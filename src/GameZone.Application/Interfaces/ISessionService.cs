using GameZone.Application.Common;
using GameZone.Application.DTOs.Sessions;

namespace GameZone.Application.Interfaces;

public interface ISessionService
{
    Task<IReadOnlyList<SessionDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionDto>> GetTodaysAsync(CancellationToken cancellationToken = default);
    Task<Result<SessionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<SessionDto>> StartAsync(StartSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<EndSessionResult>> EndAsync(int sessionId, CancellationToken cancellationToken = default);
}
