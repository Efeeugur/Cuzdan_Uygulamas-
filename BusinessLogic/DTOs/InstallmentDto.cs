using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Açıklama alanı gereklidir.")]
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Toplam tutar gereklidir.")]
    [Range(0.01, 999999.99, ErrorMessage = "Toplam tutar 0.01 ile 999,999.99 arasında olmalıdır.")]
    public decimal TotalAmount { get; set; }
    
    [Required(ErrorMessage = "Taksit sayısı gereklidir.")]
    [Range(1, 360, ErrorMessage = "Taksit sayısı 1 ile 360 ay arasında olmalıdır.")]
    public int TotalInstallments { get; set; }
    
    [Range(0, 99.99, ErrorMessage = "Faiz oranı 0 ile 99.99 arasında olmalıdır.")]
    [Display(Name = "Faiz Oranı (Aylık %)")]
    public decimal InterestRate { get; set; } = 0;
    
    [Required(ErrorMessage = "İlk ödeme tarihi gereklidir.")]
    [Display(Name = "İlk Ödeme Tarihi")]
    public DateTime FirstPaymentDate { get; set; } = DateTime.UtcNow;
    
    [Required(ErrorMessage = "Kategori seçimi gereklidir.")]
    [Range(26, 35, ErrorMessage = "Geçerli bir taksit kategorisi seçiniz.")]
    public int CategoryId { get; set; }
}

public class UpdateInstallmentDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public int? CategoryId { get; set; }
}