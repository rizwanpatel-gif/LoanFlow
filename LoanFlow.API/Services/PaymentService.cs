using LoanFlow.API.Data;
using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.API.Services;

public class PaymentService : IPaymentService
{
    private readonly LoanFlowDbContext _db;

    public PaymentService(LoanFlowDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PaymentResponse>> GenerateScheduleAsync(Guid loanApplicationId)
    {
        var loan = await _db.LoanApplications.FindAsync(loanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application {loanApplicationId} not found");

        if (loan.Status != LoanStatus.Approved && loan.Status != LoanStatus.Disbursed)
            throw new InvalidOperationException("Payment schedule can only be generated for approved loans");

        var existingPayments = await _db.Payments.AnyAsync(p => p.LoanApplicationId == loanApplicationId);
        if (existingPayments)
            throw new InvalidOperationException("Payment schedule already exists for this loan");

        decimal principal = loan.ApprovedAmount ?? loan.RequestedAmount;
        decimal annualRate = loan.InterestRate ?? 10.0m;
        decimal monthlyRate = annualRate / 100 / 12;
        int termMonths = loan.TermMonths;

        decimal monthlyPayment;
        if (monthlyRate == 0)
        {
            monthlyPayment = principal / termMonths;
        }
        else
        {
            double r = (double)monthlyRate;
            double factor = Math.Pow(1 + r, termMonths);
            monthlyPayment = principal * (decimal)(r * factor / (factor - 1));
        }

        monthlyPayment = Math.Round(monthlyPayment, 2);
        decimal remainingBalance = principal;
        var payments = new List<Payment>();

        for (int i = 1; i <= termMonths; i++)
        {
            decimal interestAmount = Math.Round(remainingBalance * monthlyRate, 2);
            decimal principalAmount = monthlyPayment - interestAmount;

            if (i == termMonths)
            {
                principalAmount = remainingBalance;
                monthlyPayment = principalAmount + interestAmount;
            }

            remainingBalance -= principalAmount;
            if (remainingBalance < 0) remainingBalance = 0;

            payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = loanApplicationId,
                Amount = monthlyPayment,
                PrincipalAmount = principalAmount,
                InterestAmount = interestAmount,
                DueDate = DateTime.UtcNow.AddMonths(i),
                PaymentNumber = i,
                RemainingBalance = remainingBalance,
                Status = PaymentStatus.Scheduled
            });
        }

        loan.Status = LoanStatus.Disbursed;
        loan.UpdatedAt = DateTime.UtcNow;

        _db.Payments.AddRange(payments);
        await _db.SaveChangesAsync();

        return payments.Select(MapToResponse);
    }

    public async Task<IEnumerable<PaymentResponse>> GetByLoanIdAsync(Guid loanApplicationId)
    {
        return await _db.Payments
            .Where(p => p.LoanApplicationId == loanApplicationId)
            .OrderBy(p => p.PaymentNumber)
            .Select(p => MapToResponse(p))
            .ToListAsync();
    }

    public async Task<PaymentResponse?> MakePaymentAsync(Guid paymentId, MakePaymentRequest request)
    {
        var payment = await _db.Payments
            .Include(p => p.LoanApplication)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null) return null;

        if (payment.Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Payment has already been completed");

        if (request.Amount < payment.Amount)
            throw new InvalidOperationException($"Payment amount must be at least {payment.Amount}");

        payment.Status = PaymentStatus.Completed;
        payment.PaidDate = DateTime.UtcNow;

        bool allPaid = await _db.Payments
            .Where(p => p.LoanApplicationId == payment.LoanApplicationId && p.Id != payment.Id)
            .AllAsync(p => p.Status == PaymentStatus.Completed);

        if (allPaid)
        {
            payment.LoanApplication.Status = LoanStatus.Closed;
            payment.LoanApplication.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<PaymentSummaryResponse?> GetSummaryAsync(Guid loanApplicationId)
    {
        var payments = await _db.Payments
            .Where(p => p.LoanApplicationId == loanApplicationId)
            .ToListAsync();

        if (!payments.Any()) return null;

        var nextPayment = payments
            .Where(p => p.Status == PaymentStatus.Scheduled)
            .OrderBy(p => p.DueDate)
            .FirstOrDefault();

        return new PaymentSummaryResponse
        {
            LoanApplicationId = loanApplicationId,
            TotalPaid = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
            TotalRemaining = payments.Where(p => p.Status != PaymentStatus.Completed).Sum(p => p.Amount),
            CompletedPayments = payments.Count(p => p.Status == PaymentStatus.Completed),
            RemainingPayments = payments.Count(p => p.Status != PaymentStatus.Completed),
            NextDueDate = nextPayment?.DueDate,
            NextPaymentAmount = nextPayment?.Amount
        };
    }

    private static PaymentResponse MapToResponse(Payment p) => new()
    {
        Id = p.Id,
        LoanApplicationId = p.LoanApplicationId,
        Amount = p.Amount,
        PrincipalAmount = p.PrincipalAmount,
        InterestAmount = p.InterestAmount,
        DueDate = p.DueDate,
        PaidDate = p.PaidDate,
        Status = p.Status.ToString(),
        PaymentNumber = p.PaymentNumber,
        RemainingBalance = p.RemainingBalance
    };
}
