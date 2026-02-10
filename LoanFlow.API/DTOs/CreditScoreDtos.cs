namespace LoanFlow.API.DTOs;

public record CreditScoreResponse
{
    public Guid Id { get; init; }
    public Guid BorrowerId { get; init; }
    public string BorrowerName { get; init; } = string.Empty;
    public int Score { get; init; }
    public string Rating { get; init; } = string.Empty;
    public decimal DebtToIncomeRatio { get; init; }
    public int OpenAccounts { get; init; }
    public int Delinquencies { get; init; }
    public DateTime ScoreDate { get; init; }
}

public record CreditCheckRequest
{
    public decimal MonthlyDebt { get; init; }
    public int OpenAccounts { get; init; }
    public int Delinquencies { get; init; }
}
