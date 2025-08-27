using Cüzdan_Uygulaması.Models;

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

public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }
    
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public int? InstallmentId { get; set; }
}

public class UpdateTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }
    
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }
    public int? InstallmentId { get; set; }
}

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetAmount => TotalIncome - TotalExpense;
    public int TransactionCount { get; set; }
}