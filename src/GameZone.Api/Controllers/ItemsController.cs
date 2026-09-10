using GameZone.Application.DTOs.GamingItems;
using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize]
public class ItemsController : ApiControllerBase
{
    private readonly IGamingItemService _itemService;

    public ItemsController(IGamingItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _itemService.GetAllAsync(cancellationToken));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
        => Ok(await _itemService.GetActiveAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GamingItemDto dto, CancellationToken cancellationToken)
        => FromResult(await _itemService.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GamingItemDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return FromResult(await _itemService.UpdateAsync(dto, cancellationToken));
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => FromResult(await _itemService.SetActiveAsync(id, true, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        => FromResult(await _itemService.SetActiveAsync(id, false, cancellationToken));

    [HttpDelete("{id:int}")]
    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => FromResult(await _itemService.DeleteAsync(id, cancellationToken));
}
