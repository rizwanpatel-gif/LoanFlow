using LoanFlow.API.DTOs;
using LoanFlow.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpPost("loan/{loanApplicationId:guid}/generate")]
    public async Task<IActionResult> GenerateSchedule(Guid loanApplicationId)
    {
        try
        {
            var result = await _service.GenerateScheduleAsync(loanApplicationId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("loan/{loanApplicationId:guid}")]
    public async Task<IActionResult> GetByLoan(Guid loanApplicationId)
    {
        var result = await _service.GetByLoanIdAsync(loanApplicationId);
        return Ok(result);
    }

    [HttpPost("{paymentId:guid}/pay")]
    public async Task<IActionResult> MakePayment(Guid paymentId, [FromBody] MakePaymentRequest request)
    {
        try
        {
            var result = await _service.MakePaymentAsync(paymentId, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("loan/{loanApplicationId:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid loanApplicationId)
    {
        var result = await _service.GetSummaryAsync(loanApplicationId);
        return result is null ? NotFound() : Ok(result);
    }
}
