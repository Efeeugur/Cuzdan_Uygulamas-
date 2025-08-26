using System.ComponentModel.DataAnnotations;

namespace Cüzdan_Uygulaması.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(7)]
    public string? Color { get; set; }

    public CategoryType Type { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum CategoryType
{
    Income = 0,
    Expense = 1
}