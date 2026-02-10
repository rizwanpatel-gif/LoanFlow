using LoanFlow.API.DTOs;
using LoanFlow.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditScoresController : ControllerBase
{
    private readonly ICreditScoreService _service;

    public CreditScoresController(ICreditScoreService service)
    {
        _service = service;
    }

    [HttpPost("borrower/{borrowerId:guid}/check")]
    public async Task<IActionResult> RunCreditCheck(Guid borrowerId, [FromBody] CreditCheckRequest request)
    {
        try
        {
            var result = await _service.RunCreditCheckAsync(borrowerId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("borrower/{borrowerId:guid}")]
    public async Task<IActionResult> GetByBorrower(Guid borrowerId)
    {
        var result = await _service.GetByBorrowerIdAsync(borrowerId);
        return result is null ? NotFound() : Ok(result);
    }
}
