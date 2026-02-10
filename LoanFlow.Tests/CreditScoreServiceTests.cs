using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using LoanFlow.API.Services;

namespace LoanFlow.Tests;

public class CreditScoreServiceTests
{
    [Theory]
    [InlineData(120000, 15, 5, 0, 750)]
    [InlineData(80000, 30, 5, 0, 650)]
    [InlineData(40000, 45, 5, 0, 525)]
    [InlineData(20000, 55, 2, 3, 300)]
    public void CalculateScore_ShouldReturnExpectedRange(decimal income, decimal dti, int accounts, int delinquencies, int expected)
    {
        var score = CreditScoreService.CalculateScore(income, dti, accounts, delinquencies);
        Assert.Equal(expected, score);
    }

    [Theory]
    [InlineData(800, "Excellent")]
    [InlineData(750, "Excellent")]
    [InlineData(720, "Good")]
    [InlineData(650, "Fair")]
    [InlineData(600, "Poor")]
    [InlineData(500, "Very Poor")]
    [InlineData(300, "Very Poor")]
    public void GetRating_ShouldReturnCorrectRating(int score, string expected)
    {
        var rating = CreditScoreService.GetRating(score);
        Assert.Equal(expected, rating);
    }

    [Fact]
    public void CalculateScore_ShouldClampBetween300And850()
    {
        var lowScore = CreditScoreService.CalculateScore(0, 100, 0, 20);
        var highScore = CreditScoreService.CalculateScore(200000, 5, 5, 0);

        Assert.True(lowScore >= 300);
        Assert.True(highScore <= 850);
    }

    [Fact]
    public async Task RunCreditCheckAsync_ShouldCreateScore()
    {
        var db = TestDbHelper.CreateContext();
        var service = new CreditScoreService(db);

        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "5551234567",
            Ssn = "123456789",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Address = "123 Test St",
            AnnualIncome = 80000,
            EmploymentStatus = "Employed"
        };
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync();

        var result = await service.RunCreditCheckAsync(borrower.Id, new CreditCheckRequest
        {
            MonthlyDebt = 1500,
            OpenAccounts = 5,
            Delinquencies = 0
        });

        Assert.Equal(borrower.Id, result.BorrowerId);
        Assert.True(result.Score >= 300 && result.Score <= 850);
        Assert.NotEmpty(result.Rating);
        Assert.Equal("Test User", result.BorrowerName);
    }

    [Fact]
    public async Task RunCreditCheckAsync_NonExistingBorrower_ShouldThrow()
    {
        var db = TestDbHelper.CreateContext();
        var service = new CreditScoreService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.RunCreditCheckAsync(Guid.NewGuid(), new CreditCheckRequest
            {
                MonthlyDebt = 1000,
                OpenAccounts = 3,
                Delinquencies = 0
            }));
    }

    [Fact]
    public async Task RunCreditCheckAsync_ShouldReplaceExistingScore()
    {
        var db = TestDbHelper.CreateContext();
        var service = new CreditScoreService(db);

        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = "Replace",
            LastName = "Test",
            Email = "replace@example.com",
            Phone = "5559999999",
            Ssn = "999888777",
            DateOfBirth = new DateOnly(1985, 6, 15),
            Address = "456 Replace Ave",
            AnnualIncome = 100000,
            EmploymentStatus = "Employed"
        };
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync();

        var first = await service.RunCreditCheckAsync(borrower.Id, new CreditCheckRequest
        {
            MonthlyDebt = 2000,
            OpenAccounts = 3,
            Delinquencies = 1
        });

        var second = await service.RunCreditCheckAsync(borrower.Id, new CreditCheckRequest
        {
            MonthlyDebt = 500,
            OpenAccounts = 5,
            Delinquencies = 0
        });

        Assert.NotEqual(first.Id, second.Id);
        Assert.True(second.Score > first.Score);
    }
}
