using GameZone.Application.Common;
using GameZone.Application.DTOs.Clients;

namespace GameZone.Application.Interfaces;

public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientDto>> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> CreateAsync(ClientEditorDto dto, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> UpdateAsync(ClientEditorDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ClientHistoryDto>> GetHistoryAsync(int clientId, CancellationToken cancellationToken = default);
}
