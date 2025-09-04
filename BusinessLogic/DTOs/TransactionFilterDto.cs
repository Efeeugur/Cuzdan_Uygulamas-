using System.ComponentModel.DataAnnotations;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class TransactionFilterDto
{
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }
    
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }
    
    [Display(Name = "Account")]
    public int? AccountId { get; set; }
    
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    
    [Display(Name = "Transaction Type")]
    public TransactionType? Type { get; set; }
    
    [Display(Name = "Recurring Only")]
    public bool? IsRecurring { get; set; }
    
    [Display(Name = "Search")]
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? Search { get; set; }
    
    [Display(Name = "Only Installments")]
    public bool OnlyInstallments { get; set; }
}