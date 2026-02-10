using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanFlow.API.Models;

[Table("credit_scores")]
public class CreditScore
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("borrower_id")]
    public Guid BorrowerId { get; set; }

    [ForeignKey("BorrowerId")]
    public Borrower Borrower { get; set; } = null!;

    [Column("score")]
    public int Score { get; set; }

    [Column("score_date")]
    public DateTime ScoreDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    [Column("rating")]
    public string Rating { get; set; } = string.Empty;

    [Column("debt_to_income_ratio")]
    public decimal DebtToIncomeRatio { get; set; }

    [Column("open_accounts")]
    public int OpenAccounts { get; set; }

    [Column("delinquencies")]
    public int Delinquencies { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
