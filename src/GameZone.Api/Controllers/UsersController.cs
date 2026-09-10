using GameZone.Api.Contracts;
using GameZone.Application.DTOs.Users;
using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _userService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserEditorDto dto, CancellationToken cancellationToken)
        => FromResult(await _userService.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserEditorDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        return FromResult(await _userService.UpdateAsync(dto, cancellationToken));
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
        => FromResult(await _userService.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        => FromResult(await _userService.DeactivateAsync(id, cancellationToken));

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _userService.ResetPasswordAsync(id, request.NewPassword, cancellationToken));
}
