namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class InstallmentDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TotalInstallments { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public int RemainingInstallments { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    
    public UserDto? User { get; set; }
    public CategoryDto? Category { get; set; }
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    
    public decimal TotalPaid => Transactions.Sum(t => t.Amount);
    public decimal RemainingAmount => TotalAmount - TotalPaid;
}

public class CreateInstallmentDto
{
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TotalInstallments { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public DateTime FirstPaymentDate { get; set; } = DateTime.UtcNow;
    public int? CategoryId { get; set; }
}

public class UpdateInstallmentDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public int? CategoryId { get; set; }
}