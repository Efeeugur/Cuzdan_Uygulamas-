using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cüzdan_Uygulaması.Models;

public class Installment
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPayment { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; } = 0;

    public int TotalInstallments { get; set; }

    public int RemainingInstallments { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime NextPaymentDate { get; set; }

    public InstallmentStatus Status { get; set; } = InstallmentStatus.Active;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum InstallmentStatus
{
    Active = 0,
    Completed = 1
}