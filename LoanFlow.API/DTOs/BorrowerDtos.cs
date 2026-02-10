using System.ComponentModel.DataAnnotations;

namespace LoanFlow.API.DTOs;

public record CreateBorrowerRequest
{
    [Required] [MaxLength(100)] public string FirstName { get; init; } = string.Empty;
    [Required] [MaxLength(100)] public string LastName { get; init; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; init; } = string.Empty;
    [Required] [Phone] public string Phone { get; init; } = string.Empty;
    [Required] [MaxLength(11)] public string Ssn { get; init; } = string.Empty;
    [Required] public DateOnly DateOfBirth { get; init; }
    [Required] public string Address { get; init; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal AnnualIncome { get; init; }
    [Required] public string EmploymentStatus { get; init; } = string.Empty;
}

public record UpdateBorrowerRequest
{
    [MaxLength(100)] public string? FirstName { get; init; }
    [MaxLength(100)] public string? LastName { get; init; }
    [Phone] public string? Phone { get; init; }
    public string? Address { get; init; }
    [Range(0, double.MaxValue)] public decimal? AnnualIncome { get; init; }
    public string? EmploymentStatus { get; init; }
}

public record BorrowerResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public string Address { get; init; } = string.Empty;
    public decimal AnnualIncome { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
