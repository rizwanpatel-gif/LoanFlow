namespace LoanFlow.API.DTOs;

public record PaymentResponse
{
    public Guid Id { get; init; }
    public Guid LoanApplicationId { get; init; }
    public decimal Amount { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal InterestAmount { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? PaidDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public int PaymentNumber { get; init; }
    public decimal RemainingBalance { get; init; }
}

public record MakePaymentRequest
{
    public decimal Amount { get; init; }
}

public record PaymentSummaryResponse
{
    public Guid LoanApplicationId { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal TotalRemaining { get; init; }
    public int CompletedPayments { get; init; }
    public int RemainingPayments { get; init; }
    public DateTime? NextDueDate { get; init; }
    public decimal? NextPaymentAmount { get; init; }
}
