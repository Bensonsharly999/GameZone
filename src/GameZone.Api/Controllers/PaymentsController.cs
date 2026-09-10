using GameZone.Application.DTOs.Payments;
using GameZone.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _paymentService.GetAllAsync(cancellationToken));

    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken cancellationToken)
        => Ok(await _paymentService.GetPendingAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
        => FromResult(await _paymentService.RecordAsync(request, cancellationToken));

    [HttpPost("{id:int}/paid")]
    public async Task<IActionResult> MarkPaid(int id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
        => FromResult(await _paymentService.MarkPaidAsync(id, request, cancellationToken));
}
