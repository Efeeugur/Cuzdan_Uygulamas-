using System.ComponentModel.DataAnnotations;

namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class PayInstallmentDto
{
    [Required]
    public int InstallmentId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [StringLength(500)]
    public string? Notes { get; set; }
}