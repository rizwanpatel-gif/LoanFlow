using LoanFlow.API.Data;
using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.API.Services;

public class CreditScoreService : ICreditScoreService
{
    private readonly LoanFlowDbContext _db;

    public CreditScoreService(LoanFlowDbContext db)
    {
        _db = db;
    }

    public async Task<CreditScoreResponse> RunCreditCheckAsync(Guid borrowerId, CreditCheckRequest request)
    {
        var borrower = await _db.Borrowers.FindAsync(borrowerId)
            ?? throw new KeyNotFoundException($"Borrower {borrowerId} not found");

        var existing = await _db.CreditScores.FirstOrDefaultAsync(c => c.BorrowerId == borrowerId);
        if (existing is not null)
            _db.CreditScores.Remove(existing);

        decimal dti = borrower.AnnualIncome > 0
            ? (request.MonthlyDebt * 12) / borrower.AnnualIncome * 100
            : 100;

        int score = CalculateScore(borrower.AnnualIncome, dti, request.OpenAccounts, request.Delinquencies);
        string rating = GetRating(score);

        var creditScore = new CreditScore
        {
            Id = Guid.NewGuid(),
            BorrowerId = borrowerId,
            Score = score,
            Rating = rating,
            DebtToIncomeRatio = Math.Round(dti, 2),
            OpenAccounts = request.OpenAccounts,
            Delinquencies = request.Delinquencies
        };

        _db.CreditScores.Add(creditScore);
        await _db.SaveChangesAsync();

        return new CreditScoreResponse
        {
            Id = creditScore.Id,
            BorrowerId = borrowerId,
            BorrowerName = $"{borrower.FirstName} {borrower.LastName}",
            Score = creditScore.Score,
            Rating = creditScore.Rating,
            DebtToIncomeRatio = creditScore.DebtToIncomeRatio,
            OpenAccounts = creditScore.OpenAccounts,
            Delinquencies = creditScore.Delinquencies,
            ScoreDate = creditScore.ScoreDate
        };
    }

    public async Task<CreditScoreResponse?> GetByBorrowerIdAsync(Guid borrowerId)
    {
        var cs = await _db.CreditScores
            .Include(c => c.Borrower)
            .FirstOrDefaultAsync(c => c.BorrowerId == borrowerId);

        if (cs is null) return null;

        return new CreditScoreResponse
        {
            Id = cs.Id,
            BorrowerId = cs.BorrowerId,
            BorrowerName = $"{cs.Borrower.FirstName} {cs.Borrower.LastName}",
            Score = cs.Score,
            Rating = cs.Rating,
            DebtToIncomeRatio = cs.DebtToIncomeRatio,
            OpenAccounts = cs.OpenAccounts,
            Delinquencies = cs.Delinquencies,
            ScoreDate = cs.ScoreDate
        };
    }

    public static int CalculateScore(decimal annualIncome, decimal dti, int openAccounts, int delinquencies)
    {
        int score = 300;

        if (annualIncome >= 100000) score += 200;
        else if (annualIncome >= 60000) score += 150;
        else if (annualIncome >= 30000) score += 100;
        else score += 50;

        if (dti < 20) score += 200;
        else if (dti < 36) score += 150;
        else if (dti < 50) score += 75;

        if (openAccounts >= 3 && openAccounts <= 10) score += 50;
        else if (openAccounts > 10) score += 20;

        score -= delinquencies * 50;

        return Math.Clamp(score, 300, 850);
    }

    public static string GetRating(int score) => score switch
    {
        >= 750 => "Excellent",
        >= 700 => "Good",
        >= 650 => "Fair",
        >= 600 => "Poor",
        _ => "Very Poor"
    };
}
