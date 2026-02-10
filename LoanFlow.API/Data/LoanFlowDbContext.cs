using LoanFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanFlow.API.Data;

public class LoanFlowDbContext : DbContext
{
    public LoanFlowDbContext(DbContextOptions<LoanFlowDbContext> options) : base(options) { }

    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<CreditScore> CreditScores => Set<CreditScore>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.HasIndex(b => b.Email).IsUnique();
            entity.HasIndex(b => b.Ssn).IsUnique();
            entity.Property(b => b.AnnualIncome).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasIndex(l => l.BorrowerId);
            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.ApplicationDate);
            entity.Property(l => l.RequestedAmount).HasPrecision(18, 2);
            entity.Property(l => l.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(l => l.InterestRate).HasPrecision(5, 2);
        });

        modelBuilder.Entity<CreditScore>(entity =>
        {
            entity.HasIndex(c => c.BorrowerId).IsUnique();
            entity.Property(c => c.DebtToIncomeRatio).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.LoanApplicationId);
            entity.HasIndex(p => p.DueDate);
            entity.HasIndex(p => p.Status);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(p => p.InterestAmount).HasPrecision(18, 2);
            entity.Property(p => p.RemainingBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LoanApplication>()
            .Property(l => l.LoanType)
            .HasConversion<string>();

        modelBuilder.Entity<LoanApplication>()
            .Property(l => l.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();
    }
}
