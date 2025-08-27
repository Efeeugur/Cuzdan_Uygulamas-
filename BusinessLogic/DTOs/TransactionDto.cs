using System.ComponentModel.DataAnnotations;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.BusinessLogic.Attributes;

namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }
    public DateTime? NextRecurrenceDate { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public int? InstallmentId { get; set; }
    
    public UserDto? User { get; set; }
    public AccountDto? Account { get; set; }
    public CategoryDto? Category { get; set; }
    public InstallmentDto? Installment { get; set; }
}

[RecurrenceValidation]
public class CreateTransactionDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Transaction date is required")]
    [Display(Name = "Transaction Date")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    [Required(ErrorMessage = "Transaction type is required")]
    [Display(Name = "Transaction Type")]
    public TransactionType Type { get; set; }
    
    [Display(Name = "Is Recurring")]
    public bool IsRecurring { get; set; }
    
    [Display(Name = "Recurrence Type")]
    public RecurrenceType? RecurrenceType { get; set; }
    
    [Required(ErrorMessage = "Account is required")]
    [Display(Name = "Account")]
    public int AccountId { get; set; }
    
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    
    [Display(Name = "Installment")]
    public int? InstallmentId { get; set; }
}

[RecurrenceValidation]
public class UpdateTransactionDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Transaction date is required")]
    [Display(Name = "Transaction Date")]
    public DateTime TransactionDate { get; set; }
    
    [Required(ErrorMessage = "Transaction type is required")]
    [Display(Name = "Transaction Type")]
    public TransactionType Type { get; set; }
    
    [Display(Name = "Is Recurring")]
    public bool IsRecurring { get; set; }
    
    [Display(Name = "Recurrence Type")]
    public RecurrenceType? RecurrenceType { get; set; }
    
    [Required(ErrorMessage = "Account is required")]
    [Display(Name = "Account")]
    public int AccountId { get; set; }
    
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    
    [Display(Name = "Installment")]
    public int? InstallmentId { get; set; }
}

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetAmount => TotalIncome - TotalExpense;
    public int TransactionCount { get; set; }
}