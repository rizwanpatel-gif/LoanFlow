using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using LoanFlow.API.Services;

namespace LoanFlow.Tests;

public class LoanApplicationServiceTests
{
    private async Task<(LoanApplicationService service, Guid borrowerId)> SetupAsync()
    {
        var db = TestDbHelper.CreateContext();
        var service = new LoanApplicationService(db);

        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = "Loan",
            LastName = "Tester",
            Email = "loan@example.com",
            Phone = "5551234567",
            Ssn = "123456789",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Address = "123 Loan St",
            AnnualIncome = 80000,
            EmploymentStatus = "Employed"
        };
        db.Borrowers.Add(borrower);
        await db.SaveChangesAsync();

        return (service, borrower.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateLoanWithPendingStatus()
    {
        var (service, borrowerId) = await SetupAsync();

        var result = await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Personal,
            RequestedAmount = 25000,
            TermMonths = 36,
            Purpose = "Home renovation"
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(25000, result.RequestedAmount);
        Assert.Equal("Loan Tester", result.BorrowerName);
    }

    [Fact]
    public async Task CreateAsync_NonExistingBorrower_ShouldThrow()
    {
        var db = TestDbHelper.CreateContext();
        var service = new LoanApplicationService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateLoanApplicationRequest
            {
                BorrowerId = Guid.NewGuid(),
                LoanType = LoanType.Auto,
                RequestedAmount = 15000,
                TermMonths = 60,
                Purpose = "Car purchase"
            }));
    }

    [Fact]
    public async Task ProcessDecisionAsync_Approve_ShouldSetApproved()
    {
        var (service, borrowerId) = await SetupAsync();

        var loan = await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Mortgage,
            RequestedAmount = 300000,
            TermMonths = 360,
            Purpose = "Home purchase"
        });

        var result = await service.ProcessDecisionAsync(loan.Id, new LoanDecisionRequest
        {
            Approved = true,
            ApprovedAmount = 280000,
            InterestRate = 6.5m
        });

        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
        Assert.Equal(280000, result.ApprovedAmount);
        Assert.Equal(6.5m, result.InterestRate);
        Assert.NotNull(result.DecisionDate);
    }

    [Fact]
    public async Task ProcessDecisionAsync_Reject_ShouldSetRejected()
    {
        var (service, borrowerId) = await SetupAsync();

        var loan = await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Business,
            RequestedAmount = 500000,
            TermMonths = 120,
            Purpose = "Business expansion"
        });

        var result = await service.ProcessDecisionAsync(loan.Id, new LoanDecisionRequest
        {
            Approved = false,
            RejectionReason = "Insufficient credit history"
        });

        Assert.NotNull(result);
        Assert.Equal("Rejected", result.Status);
        Assert.Equal("Insufficient credit history", result.RejectionReason);
    }

    [Fact]
    public async Task ProcessDecisionAsync_AlreadyApproved_ShouldThrow()
    {
        var (service, borrowerId) = await SetupAsync();

        var loan = await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Personal,
            RequestedAmount = 10000,
            TermMonths = 24,
            Purpose = "Debt consolidation"
        });

        await service.ProcessDecisionAsync(loan.Id, new LoanDecisionRequest { Approved = true });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProcessDecisionAsync(loan.Id, new LoanDecisionRequest { Approved = false }));
    }

    [Fact]
    public async Task GetByBorrowerIdAsync_ShouldReturnBorrowerLoans()
    {
        var (service, borrowerId) = await SetupAsync();

        await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Personal,
            RequestedAmount = 5000,
            TermMonths = 12,
            Purpose = "Vacation"
        });

        await service.CreateAsync(new CreateLoanApplicationRequest
        {
            BorrowerId = borrowerId,
            LoanType = LoanType.Auto,
            RequestedAmount = 20000,
            TermMonths = 48,
            Purpose = "New car"
        });

        var results = await service.GetByBorrowerIdAsync(borrowerId);
        Assert.Equal(2, results.Count());
    }
}
