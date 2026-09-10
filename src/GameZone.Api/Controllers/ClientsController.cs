using GameZone.Application.DTOs.Clients;
using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize]
public class ClientsController : ApiControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var clients = string.IsNullOrWhiteSpace(search)
            ? await _clientService.GetAllAsync(cancellationToken)
            : await _clientService.SearchAsync(search, cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => FromResult(await _clientService.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> History(int id, CancellationToken cancellationToken)
        => FromResult(await _clientService.GetHistoryAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientEditorDto dto, CancellationToken cancellationToken)
        => FromResult(await _clientService.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientEditorDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return FromResult(await _clientService.UpdateAsync(dto, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _clientService.DeleteAsync(id, cancellationToken));
}
