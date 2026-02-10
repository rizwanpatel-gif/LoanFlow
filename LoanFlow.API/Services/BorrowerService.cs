using LoanFlow.API.Data;
using LoanFlow.API.DTOs;
using LoanFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.API.Services;

public class BorrowerService : IBorrowerService
{
    private readonly LoanFlowDbContext _db;

    public BorrowerService(LoanFlowDbContext db)
    {
        _db = db;
    }

    public async Task<BorrowerResponse> CreateAsync(CreateBorrowerRequest request)
    {
        var borrower = new Borrower
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Ssn = request.Ssn,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            AnnualIncome = request.AnnualIncome,
            EmploymentStatus = request.EmploymentStatus
        };

        _db.Borrowers.Add(borrower);
        await _db.SaveChangesAsync();
        return MapToResponse(borrower);
    }

    public async Task<BorrowerResponse?> GetByIdAsync(Guid id)
    {
        var borrower = await _db.Borrowers.FindAsync(id);
        return borrower is null ? null : MapToResponse(borrower);
    }

    public async Task<IEnumerable<BorrowerResponse>> GetAllAsync(int page, int pageSize)
    {
        return await _db.Borrowers
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToResponse(b))
            .ToListAsync();
    }

    public async Task<BorrowerResponse?> UpdateAsync(Guid id, UpdateBorrowerRequest request)
    {
        var borrower = await _db.Borrowers.FindAsync(id);
        if (borrower is null) return null;

        if (request.FirstName is not null) borrower.FirstName = request.FirstName;
        if (request.LastName is not null) borrower.LastName = request.LastName;
        if (request.Phone is not null) borrower.Phone = request.Phone;
        if (request.Address is not null) borrower.Address = request.Address;
        if (request.AnnualIncome.HasValue) borrower.AnnualIncome = request.AnnualIncome.Value;
        if (request.EmploymentStatus is not null) borrower.EmploymentStatus = request.EmploymentStatus;

        borrower.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(borrower);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var borrower = await _db.Borrowers.FindAsync(id);
        if (borrower is null) return false;

        _db.Borrowers.Remove(borrower);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BorrowerResponse MapToResponse(Borrower b) => new()
    {
        Id = b.Id,
        FirstName = b.FirstName,
        LastName = b.LastName,
        Email = b.Email,
        Phone = b.Phone,
        DateOfBirth = b.DateOfBirth,
        Address = b.Address,
        AnnualIncome = b.AnnualIncome,
        EmploymentStatus = b.EmploymentStatus,
        CreatedAt = b.CreatedAt
    };
}
