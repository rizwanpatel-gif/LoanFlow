using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanFlow.API.Models;

[Table("borrowers")]
public class Borrower
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(11)]
    [Column("ssn")]
    public string Ssn { get; set; } = string.Empty;

    [Column("date_of_birth")]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("annual_income")]
    public decimal AnnualIncome { get; set; }

    [Column("employment_status")]
    [MaxLength(50)]
    public string EmploymentStatus { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public CreditScore? CreditScore { get; set; }
}
