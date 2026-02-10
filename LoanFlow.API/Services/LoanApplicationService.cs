using LoanFlow.API.Data;
using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.API.Services;

public class LoanApplicationService : ILoanApplicationService
{
    private readonly LoanFlowDbContext _db;

    public LoanApplicationService(LoanFlowDbContext db)
    {
        _db = db;
    }

    public async Task<LoanApplicationResponse> CreateAsync(CreateLoanApplicationRequest request)
    {
        var borrower = await _db.Borrowers.FindAsync(request.BorrowerId)
            ?? throw new KeyNotFoundException($"Borrower {request.BorrowerId} not found");

        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            BorrowerId = request.BorrowerId,
            LoanType = request.LoanType,
            RequestedAmount = request.RequestedAmount,
            TermMonths = request.TermMonths,
            Purpose = request.Purpose,
            Status = LoanStatus.Pending
        };

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();

        return MapToResponse(loan, borrower);
    }

    public async Task<LoanApplicationResponse?> GetByIdAsync(Guid id)
    {
        var loan = await _db.LoanApplications
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id);

        return loan is null ? null : MapToResponse(loan, loan.Borrower);
    }

    public async Task<IEnumerable<LoanApplicationResponse>> GetAllAsync(int page, int pageSize, string? status)
    {
        var query = _db.LoanApplications.Include(l => l.Borrower).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus);

        return await query
            .OrderByDescending(l => l.ApplicationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => MapToResponse(l, l.Borrower))
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanApplicationResponse>> GetByBorrowerIdAsync(Guid borrowerId)
    {
        return await _db.LoanApplications
            .Include(l => l.Borrower)
            .Where(l => l.BorrowerId == borrowerId)
            .OrderByDescending(l => l.ApplicationDate)
            .Select(l => MapToResponse(l, l.Borrower))
            .ToListAsync();
    }

    public async Task<LoanApplicationResponse?> ProcessDecisionAsync(Guid id, LoanDecisionRequest request)
    {
        var loan = await _db.LoanApplications
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null) return null;
        if (loan.Status != LoanStatus.Pending && loan.Status != LoanStatus.UnderReview)
            throw new InvalidOperationException("Loan application is not in a reviewable state");

        if (request.Approved)
        {
            loan.Status = LoanStatus.Approved;
            loan.ApprovedAmount = request.ApprovedAmount ?? loan.RequestedAmount;
            loan.InterestRate = request.InterestRate ?? CalculateInterestRate(loan);
        }
        else
        {
            loan.Status = LoanStatus.Rejected;
            loan.RejectionReason = request.RejectionReason ?? "Application did not meet approval criteria";
        }

        loan.DecisionDate = DateTime.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToResponse(loan, loan.Borrower);
    }

    private static decimal CalculateInterestRate(LoanApplication loan)
    {
        decimal baseRate = loan.LoanType switch
        {
            LoanType.Mortgage => 6.5m,
            LoanType.Auto => 7.5m,
            LoanType.Personal => 10.0m,
            LoanType.Business => 8.5m,
            LoanType.Education => 5.5m,
            _ => 10.0m
        };

        if (loan.RequestedAmount > 500000) baseRate += 0.5m;
        if (loan.TermMonths > 120) baseRate += 0.25m;

        return baseRate;
    }

    private static LoanApplicationResponse MapToResponse(LoanApplication loan, Borrower borrower) => new()
    {
        Id = loan.Id,
        BorrowerId = loan.BorrowerId,
        BorrowerName = $"{borrower.FirstName} {borrower.LastName}",
        LoanType = loan.LoanType.ToString(),
        RequestedAmount = loan.RequestedAmount,
        ApprovedAmount = loan.ApprovedAmount,
        InterestRate = loan.InterestRate,
        TermMonths = loan.TermMonths,
        Status = loan.Status.ToString(),
        Purpose = loan.Purpose,
        ApplicationDate = loan.ApplicationDate,
        DecisionDate = loan.DecisionDate,
        RejectionReason = loan.RejectionReason
    };
}
