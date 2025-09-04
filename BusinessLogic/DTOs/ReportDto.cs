using System.ComponentModel.DataAnnotations;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.DTOs;

public class ReportRequestDto
{
    [Display(Name = "Başlangıç Tarihi")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Bitiş Tarihi")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Hesap")]
    public int? AccountId { get; set; }

    [Display(Name = "Kategori")]
    public int? CategoryId { get; set; }

    [Display(Name = "İşlem Türü")]
    public TransactionType? TransactionType { get; set; }
    
    [Display(Name = "Sadece Taksitler")]
    public bool OnlyInstallments { get; set; }

    [Display(Name = "Rapor Türü")]
    public ReportType ReportType { get; set; } = ReportType.Summary;

    [Display(Name = "Taksit İşlemleri Dahil")]
    public bool IncludeInstallments { get; set; } = true;

    [Display(Name = "Düzenli İşlemler Dahil")]
    public bool IncludeRecurring { get; set; } = true;

    [Display(Name = "Minimum Tutar")]
    [Range(0, 999999.99, ErrorMessage = "Minimum tutar 0 ile 999,999.99 arasında olmalıdır.")]
    public decimal? MinAmount { get; set; }

    [Display(Name = "Maksimum Tutar")]
    [Range(0, 999999.99, ErrorMessage = "Maksimum tutar 0 ile 999,999.99 arasında olmalıdır.")]
    public decimal? MaxAmount { get; set; }

    [Display(Name = "Arama")]
    [StringLength(100, ErrorMessage = "Arama terimi 100 karakteri geçemez")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Hızlı Tarih Seçimi")]
    public DateRangePreset DateRangePreset { get; set; } = DateRangePreset.Custom;
}

public class ReportResultDto
{
    public ReportRequestDto ReportRequest { get; set; } = new();
    public List<TransactionDto> Transactions { get; set; } = new();
    public TransactionSummaryDto Summary { get; set; } = new();
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
    public List<AccountBreakdownDto> AccountBreakdown { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
    public int TotalTransactions { get; set; }
    public string DateRange { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class CategoryBreakdownDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = "#007bff"; // Default bootstrap blue
}

public class AccountBreakdownDto
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
}

public class MonthlyTrendDto
{
    public string MonthYear { get; set; } = string.Empty;
    public DateTime Month { get; set; }
    public decimal IncomeTotal { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
}

public enum ReportType
{
    Summary = 0,
    Detailed = 1,
    CategoryAnalysis = 2,
    AccountAnalysis = 3,
    IncomeAnalysis = 4,
    ExpenseAnalysis = 5
}

public enum DateRangePreset
{
    Custom = 0,
    Today = 1,
    Yesterday = 2,
    ThisWeek = 3,
    LastWeek = 4,
    ThisMonth = 5,
    LastMonth = 6,
    Last3Months = 7,
    Last6Months = 8,
    ThisYear = 9,
    LastYear = 10
}

public class PdfExportOptionsDto
{
    public string Format { get; set; } = "A4";
    public bool Landscape { get; set; } = false;
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeTransactionDetails { get; set; } = true;
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
}