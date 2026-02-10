using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanFlow.API.Models;

public enum PaymentStatus
{
    Scheduled,
    Completed,
    Failed,
    Late
}

[Table("payments")]
public class Payment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("loan_application_id")]
    public Guid LoanApplicationId { get; set; }

    [ForeignKey("LoanApplicationId")]
    public LoanApplication LoanApplication { get; set; } = null!;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("principal_amount")]
    public decimal PrincipalAmount { get; set; }

    [Column("interest_amount")]
    public decimal InterestAmount { get; set; }

    [Column("due_date")]
    public DateTime DueDate { get; set; }

    [Column("paid_date")]
    public DateTime? PaidDate { get; set; }

    [Column("status")]
    public PaymentStatus Status { get; set; } = PaymentStatus.Scheduled;

    [Column("payment_number")]
    public int PaymentNumber { get; set; }

    [Column("remaining_balance")]
    public decimal RemainingBalance { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
