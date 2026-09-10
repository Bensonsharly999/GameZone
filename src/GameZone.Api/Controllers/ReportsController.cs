using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> Daily([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
        => Ok(await _reportService.GetDailyRevenueAsync(from, to, cancellationToken));

    [HttpGet("items")]
    public async Task<IActionResult> Items([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
        => Ok(await _reportService.GetRevenueByItemAsync(from, to, cancellationToken));

    [HttpGet("top-clients")]
    public async Task<IActionResult> TopClients([FromQuery] int take = 10, CancellationToken cancellationToken = default)
        => Ok(await _reportService.GetTopClientsAsync(take, cancellationToken));

    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken cancellationToken)
        => Ok(await _reportService.GetPendingPaymentsAsync(cancellationToken));
}
