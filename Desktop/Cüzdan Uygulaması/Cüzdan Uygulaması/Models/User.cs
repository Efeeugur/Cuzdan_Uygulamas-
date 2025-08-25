using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cüzdan_Uygulaması.Models;

public class User : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Installment> Installments { get; set; } = new List<Installment>();
}