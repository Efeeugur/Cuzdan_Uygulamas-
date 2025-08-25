using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cüzdan_Uygulaması.Models;

public class Transaction
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public TransactionType Type { get; set; }

    public bool IsRecurring { get; set; } = false;

    public RecurrenceType? RecurrenceType { get; set; }

    public DateTime? NextRecurrenceDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int AccountId { get; set; }

    public int? CategoryId { get; set; }

    public int? InstallmentId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual Installment? Installment { get; set; }
}

public enum TransactionType
{
    Income = 0,
    Expense = 1
}

public enum RecurrenceType
{
    Monthly = 0,
    Weekly = 1,
    Yearly = 2
}