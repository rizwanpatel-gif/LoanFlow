using System.ComponentModel.DataAnnotations;
using LoanFlow.API.Models;

namespace LoanFlow.API.DTOs;

public record CreateLoanApplicationRequest
{
    [Required] public Guid BorrowerId { get; init; }
    [Required] public LoanType LoanType { get; init; }
    [Range(1000, 10000000)] public decimal RequestedAmount { get; init; }
    [Range(6, 360)] public int TermMonths { get; init; }
    [Required] [MaxLength(1000)] public string Purpose { get; init; } = string.Empty;
}

public record LoanApplicationResponse
{
    public Guid Id { get; init; }
    public Guid BorrowerId { get; init; }
    public string BorrowerName { get; init; } = string.Empty;
    public string LoanType { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public decimal? InterestRate { get; init; }
    public int TermMonths { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public DateTime ApplicationDate { get; init; }
    public DateTime? DecisionDate { get; init; }
    public string? RejectionReason { get; init; }
}

public record LoanDecisionRequest
{
    [Required] public bool Approved { get; init; }
    public decimal? ApprovedAmount { get; init; }
    public decimal? InterestRate { get; init; }
    [MaxLength(500)] public string? RejectionReason { get; init; }
}
