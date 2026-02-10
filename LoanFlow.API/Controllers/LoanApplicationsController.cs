using LoanFlow.API.DTOs;
using LoanFlow.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _service;

    public LoanApplicationsController(ILoanApplicationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanApplicationRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var result = await _service.GetAllAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("borrower/{borrowerId:guid}")]
    public async Task<IActionResult> GetByBorrower(Guid borrowerId)
    {
        var result = await _service.GetByBorrowerIdAsync(borrowerId);
        return Ok(result);
    }

    [HttpPost("{id:guid}/decision")]
    public async Task<IActionResult> ProcessDecision(Guid id, [FromBody] LoanDecisionRequest request)
    {
        try
        {
            var result = await _service.ProcessDecisionAsync(id, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
