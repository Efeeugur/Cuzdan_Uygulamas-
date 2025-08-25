using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cüzdan_Uygulaması.Models;

public class Account
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    Cash = 0,
    BankAccount = 1,
    CreditCard = 2,
    Investment = 3
}