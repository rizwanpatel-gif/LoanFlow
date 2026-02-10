using LoanFlow.API.Data;
using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using LoanFlow.API.Services;

namespace LoanFlow.Tests;

public class PaymentServiceTests
{
    private async Task<(PaymentService service, LoanFlowDbContext db, Guid loanId)> SetupApprovedLoanAsync()
    {
        var db = TestDbHelper.CreateContext();
        var service = new PaymentService(db);

        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = "Pay",
            LastName = "Tester",
            Email = "pay@example.com",
            Phone = "5551234567",
            Ssn = "123456789",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Address = "123 Pay St",
            AnnualIncome = 100000,
            EmploymentStatus = "Employed"
        };

        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            BorrowerId = borrower.Id,
            LoanType = LoanType.Personal,
            RequestedAmount = 12000,
            ApprovedAmount = 12000,
            InterestRate = 10.0m,
            TermMonths = 12,
            Status = LoanStatus.Approved,
            Purpose = "Test loan"
        };

        db.Borrowers.Add(borrower);
        db.LoanApplications.Add(loan);
        await db.SaveChangesAsync();

        return (service, db, loan.Id);
    }

    [Fact]
    public async Task GenerateScheduleAsync_ShouldCreatePayments()
    {
        var (service, db, loanId) = await SetupApprovedLoanAsync();

        var payments = (await service.GenerateScheduleAsync(loanId)).ToList();

        Assert.Equal(12, payments.Count);
        Assert.All(payments, p => Assert.Equal("Scheduled", p.Status));
        Assert.Equal(1, payments.First().PaymentNumber);
        Assert.Equal(12, payments.Last().PaymentNumber);
        Assert.True(payments.Last().RemainingBalance <= 0.01m);
    }

    [Fact]
    public async Task GenerateScheduleAsync_PendingLoan_ShouldThrow()
    {
        var db = TestDbHelper.CreateContext();
        var service = new PaymentService(db);

        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = "Pending",
            LastName = "Loan",
            Email = "pending@example.com",
            Phone = "5550000000",
            Ssn = "000000000",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Address = "000 Pending St",
            AnnualIncome = 50000,
            EmploymentStatus = "Employed"
        };

        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            BorrowerId = borrower.Id,
            LoanType = LoanType.Personal,
            RequestedAmount = 5000,
            TermMonths = 12,
            Status = LoanStatus.Pending,
            Purpose = "Pending test"
        };

        db.Borrowers.Add(borrower);
        db.LoanApplications.Add(loan);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateScheduleAsync(loan.Id));
    }

    [Fact]
    public async Task GenerateScheduleAsync_AlreadyGenerated_ShouldThrow()
    {
        var (service, db, loanId) = await SetupApprovedLoanAsync();

        await service.GenerateScheduleAsync(loanId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateScheduleAsync(loanId));
    }

    [Fact]
    public async Task MakePaymentAsync_ShouldMarkCompleted()
    {
        var (service, db, loanId) = await SetupApprovedLoanAsync();
        var payments = (await service.GenerateScheduleAsync(loanId)).ToList();
        var firstPayment = payments.First();

        var result = await service.MakePaymentAsync(firstPayment.Id, new MakePaymentRequest
        {
            Amount = firstPayment.Amount
        });

        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotNull(result.PaidDate);
    }

    [Fact]
    public async Task MakePaymentAsync_InsufficientAmount_ShouldThrow()
    {
        var (service, db, loanId) = await SetupApprovedLoanAsync();
        var payments = (await service.GenerateScheduleAsync(loanId)).ToList();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.MakePaymentAsync(payments.First().Id, new MakePaymentRequest
            {
                Amount = 1.00m
            }));
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCorrectTotals()
    {
        var (service, db, loanId) = await SetupApprovedLoanAsync();
        var payments = (await service.GenerateScheduleAsync(loanId)).ToList();

        await service.MakePaymentAsync(payments[0].Id, new MakePaymentRequest { Amount = payments[0].Amount });
        await service.MakePaymentAsync(payments[1].Id, new MakePaymentRequest { Amount = payments[1].Amount });

        var summary = await service.GetSummaryAsync(loanId);

        Assert.NotNull(summary);
        Assert.Equal(2, summary.CompletedPayments);
        Assert.Equal(10, summary.RemainingPayments);
        Assert.True(summary.TotalPaid > 0);
        Assert.True(summary.TotalRemaining > 0);
        Assert.NotNull(summary.NextDueDate);
    }

    [Fact]
    public async Task GetSummaryAsync_NonExistingLoan_ShouldReturnNull()
    {
        var db = TestDbHelper.CreateContext();
        var service = new PaymentService(db);

        var result = await service.GetSummaryAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
