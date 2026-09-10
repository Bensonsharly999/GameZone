using GameZone.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult(Result result)
        => result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });

    protected IActionResult FromResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
}
