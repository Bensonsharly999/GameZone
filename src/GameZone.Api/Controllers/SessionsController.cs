using GameZone.Application.DTOs.Payments;
using GameZone.Application.DTOs.Sessions;
using GameZone.Application.Interfaces;
using GameZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameZone.Api.Controllers;

[Authorize]
public class SessionsController : ApiControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IPaymentService _paymentService;

    public SessionsController(ISessionService sessionService, IPaymentService paymentService)
    {
        _sessionService = sessionService;
        _paymentService = paymentService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> Active(CancellationToken cancellationToken)
        => Ok(await _sessionService.GetActiveAsync(cancellationToken));

    [HttpGet("today")]
    public async Task<IActionResult> Today(CancellationToken cancellationToken)
        => Ok(await _sessionService.GetTodaysAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => FromResult(await _sessionService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartSessionRequest request, CancellationToken cancellationToken)
        => FromResult(await _sessionService.StartAsync(request, cancellationToken));

    [HttpPost("{id:int}/end")]
    public async Task<IActionResult> End(int id, CancellationToken cancellationToken)
        => FromResult(await _sessionService.EndAsync(id, cancellationToken));

    [HttpPost("{id:int}/collect")]
    public async Task<IActionResult> Collect(int id, [FromBody] CollectPaymentRequest request, CancellationToken cancellationToken)
    {
        var ended = await _sessionService.EndAsync(id, cancellationToken);
        if (!ended.IsSuccess)
            return FromResult(ended);

        return FromResult(await _paymentService.RecordAsync(new RecordPaymentRequest
        {
            SessionId = id,
            Amount = request.PaymentMethod == PaymentMethod.Free ? 0 : ended.Value!.Amount,
            PaymentMethod = request.PaymentMethod,
            DiscountPercent = request.DiscountPercent,
            TransactionReference = request.TransactionReference,
            PaymentStatus = request.PaymentStatus
        }, cancellationToken));
    }
}
