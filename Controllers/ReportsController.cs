using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Services;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ISimpleCategoryService _simpleCategoryService;
    private readonly IPdfService _pdfService;

    public ReportsController(
        ITransactionService transactionService,
        IAccountService accountService,
        ISimpleCategoryService simpleCategoryService,
        IPdfService pdfService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _simpleCategoryService = simpleCategoryService;
        _pdfService = pdfService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Index(ReportRequestDto? request)
    {
        var userId = GetUserId();
        
        // Initialize request if null with no default date values (user flexibility)
        request ??= new ReportRequestDto
        {
            ReportType = ReportType.Summary
        };

        // Populate dropdown lists for filtering
        await PopulateDropdownsAsync(userId);

        // Get report data
        var reportResult = await GenerateReportAsync(request, userId);
        
        // Pass both request and result to view
        ViewBag.Request = request;
        
        // Store the current report request in TempData for PDF export
        TempData["CurrentReportRequest"] = System.Text.Json.JsonSerializer.Serialize(request);
        
        return View(reportResult);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateReport(ReportRequestDto request)
    {
        var userId = GetUserId();
        
        // Validate request
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(userId);
            ViewBag.Request = request;
            var emptyResult = new ReportResultDto();
            return View("Index", emptyResult);
        }

        // Generate report
        var reportResult = await GenerateReportAsync(request, userId);
        
        await PopulateDropdownsAsync(userId);
        ViewBag.Request = request;
        
        // Store the current report request in TempData for PDF export
        TempData["CurrentReportRequest"] = System.Text.Json.JsonSerializer.Serialize(request);
        
        return View("Index", reportResult);
    }

    [HttpPost]
    public async Task<JsonResult> GenerateReportAjax(ReportRequestDto request)
    {
        var userId = GetUserId();
        
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, error = "Geçersiz filtre parametreleri." });
        }

        try
        {
            var reportResult = await GenerateReportAsync(request, userId);
            
            return Json(new { 
                success = true, 
                data = new {
                    summary = reportResult.Summary,
                    categoryBreakdown = reportResult.CategoryBreakdown,
                    accountBreakdown = reportResult.AccountBreakdown,
                    monthlyTrends = reportResult.MonthlyTrends,
                    totalTransactions = reportResult.TotalTransactions,
                    dateRange = reportResult.DateRange,
                    transactions = reportResult.Transactions.Take(20) // Limit for performance
                }
            });
        }
        catch (Exception)
        {
            return Json(new { success = false, error = "Rapor oluşturulurken bir hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExportToPdf(PdfExportOptionsDto? pdfOptions = null)
    {
        var userId = GetUserId();
        
        try
        {
            // Get the current report request from TempData
            ReportRequestDto request;
            if (TempData.Peek("CurrentReportRequest") is string requestJson)
            {
                request = System.Text.Json.JsonSerializer.Deserialize<ReportRequestDto>(requestJson) ?? new ReportRequestDto();
                Console.WriteLine($"PDF EXPORT: Using stored report request");
                Console.WriteLine($"  OnlyInstallments: {request.OnlyInstallments}");
                Console.WriteLine($"  TransactionType: {request.TransactionType}");
                Console.WriteLine($"  StartDate: {request.StartDate}");
                Console.WriteLine($"  EndDate: {request.EndDate}");
            }
            else
            {
                // Fallback to empty request (all transactions)
                request = new ReportRequestDto();
                Console.WriteLine($"PDF EXPORT: No stored request found, using default (all transactions)");
            }
            
            // Generate report data using the same filters as the displayed report
            var reportResult = await GenerateReportAsync(request, userId);
            
            Console.WriteLine($"PDF EXPORT: Report generated with {reportResult.Transactions.Count} transactions");
            
            // Convert PDF options
            var options = new BusinessLogic.Interfaces.PdfExportOptions
            {
                Format = pdfOptions?.Format ?? "A4",
                Landscape = pdfOptions?.Landscape ?? false,
                IncludeCharts = pdfOptions?.IncludeCharts ?? true,
                IncludeTransactionDetails = pdfOptions?.IncludeTransactionDetails ?? true,
                HeaderText = pdfOptions?.HeaderText,
                FooterText = pdfOptions?.FooterText ?? "Cüzdan Uygulaması Finansal Raporu"
            };

            // Generate PDF
            var pdfBytes = await _pdfService.GenerateReportPdfAsync(reportResult, options);
            
            // Create filename
            var filename = $"Finansal_Rapor_{reportResult.DateRange.Replace("/", "-").Replace(" - ", "_")}.pdf";
            
            // Return file
            return File(pdfBytes, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"PDF oluşturulurken hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PreviewPdf(ReportRequestDto request)
    {
        var userId = GetUserId();
        
        try
        {
            // Generate report data
            var reportResult = await GenerateReportAsync(request, userId);
            
            // Get HTML preview
            var html = _pdfService.GetReportHtml(reportResult, includeCharts: false);
            
            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            return Content($"<html><body><h1>Hata</h1><p>{ex.Message}</p></body></html>", "text/html");
        }
    }

    private async Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId)
    {
        try
        {
            // Create filter from report request
            var filter = new TransactionFilterDto
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AccountId = request.AccountId,
                CategoryId = request.CategoryId,
                Type = request.OnlyInstallments ? null : request.TransactionType,
                IsRecurring = null, // Always include both recurring and non-recurring transactions
                Search = request.SearchTerm,
                OnlyInstallments = request.OnlyInstallments
            };

            // Get filtered transactions
            var transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
            var summary = await _transactionService.GetTransactionSummaryAsync(userId, filter);

            // Calculate additional analytics
            var result = new ReportResultDto
            {
                ReportRequest = request,
                Transactions = transactions.ToList(),
                Summary = summary,
                GeneratedDate = DateTime.UtcNow,
                TotalTransactions = transactions.Count(),
                DateRange = $"{(request.StartDate?.ToString("dd/MM/yyyy") ?? "Başlangıç yok")} - {(request.EndDate?.ToString("dd/MM/yyyy") ?? "Bitiş yok")}"
            };

            // Calculate category breakdown
            result.CategoryBreakdown = transactions
                .Where(t => t.CategoryId.HasValue)
                .GroupBy(t => new { t.CategoryId, CategoryName = _simpleCategoryService.GetCategoryName(t.CategoryId!.Value) })
                .Select(g => new CategoryBreakdownDto
                {
                    CategoryId = g.Key.CategoryId!.Value,
                    CategoryName = g.Key.CategoryName,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    Percentage = transactions.Any() ? (g.Sum(t => t.Amount) / transactions.Sum(t => Math.Abs(t.Amount))) * 100 : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            // Calculate account breakdown
            result.AccountBreakdown = transactions
                .Where(t => t.Account != null)
                .GroupBy(t => new { t.AccountId, t.Account!.Name })
                .Select(g => new AccountBreakdownDto
                {
                    AccountId = g.Key.AccountId,
                    AccountName = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(a => Math.Abs(a.TotalAmount))
                .ToList();

            return result;
        }
        catch (Exception)
        {
            // Log error and return empty result
            return new ReportResultDto
            {
                ReportRequest = request,
                Transactions = new List<TransactionDto>(),
                Summary = new TransactionSummaryDto(),
                GeneratedDate = DateTime.UtcNow,
                ErrorMessage = "Rapor oluşturulurken bir hata oluştu. Lütfen tekrar deneyin."
            };
        }
    }

    private async Task PopulateDropdownsAsync(string userId)
    {
        try
        {
            var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
            ViewBag.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            var categories = _simpleCategoryService.GetAllCategories();
            ViewBag.Categories = categories.ToList();

            ViewBag.ReportTypes = Enum.GetValues<ReportType>().Select(rt => new SelectListItem
            {
                Value = ((int)rt).ToString(),
                Text = GetReportTypeDisplayName(rt)
            }).ToList();
        }
        catch
        {
            ViewBag.Accounts = new List<SelectListItem>();
            ViewBag.Categories = new List<SelectListItem>();
            ViewBag.ReportTypes = new List<SelectListItem>();
        }
    }

    private static string GetReportTypeDisplayName(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.Summary => "Özet Raporu",
            ReportType.Detailed => "Detaylı Rapor",
            ReportType.CategoryAnalysis => "Kategori Analizi",
            ReportType.AccountAnalysis => "Hesap Analizi",
            ReportType.IncomeAnalysis => "Gelir Analizi",
            ReportType.ExpenseAnalysis => "Gider Analizi",
            _ => reportType.ToString()
        };
    }
}