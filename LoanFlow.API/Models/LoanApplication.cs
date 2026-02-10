using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanFlow.API.Models;

public enum LoanStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Disbursed,
    Closed
}

public enum LoanType
{
    Personal,
    Mortgage,
    Auto,
    Business,
    Education
}

[Table("loan_applications")]
public class LoanApplication
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("borrower_id")]
    public Guid BorrowerId { get; set; }

    [ForeignKey("BorrowerId")]
    public Borrower Borrower { get; set; } = null!;

    [Column("loan_type")]
    public LoanType LoanType { get; set; }

    [Column("requested_amount")]
    public decimal RequestedAmount { get; set; }

    [Column("approved_amount")]
    public decimal? ApprovedAmount { get; set; }

    [Column("interest_rate")]
    public decimal? InterestRate { get; set; }

    [Column("term_months")]
    public int TermMonths { get; set; }

    [Column("status")]
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    [MaxLength(1000)]
    [Column("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [Column("application_date")]
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    [Column("decision_date")]
    public DateTime? DecisionDate { get; set; }

    [MaxLength(500)]
    [Column("rejection_reason")]
    public string? RejectionReason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
