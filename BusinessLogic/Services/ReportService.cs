using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly ITransactionService _transactionService;
    private readonly ISimpleCategoryService _simpleCategoryService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public ReportService(
        ITransactionService transactionService, 
        ISimpleCategoryService simpleCategoryService,
        IMemoryCache cache)
    {
        _transactionService = transactionService;
        _simpleCategoryService = simpleCategoryService;
        _cache = cache;
    }

    public async Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId)
    {
        try
        {
            // Validate request
            var validationErrors = ValidateReportRequest(request);
            if (validationErrors.Any())
            {
                return new ReportResultDto
                {
                    ReportRequest = request,
                    ErrorMessage = string.Join("; ", validationErrors),
                    GeneratedDate = DateTime.UtcNow
                };
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(request, userId);
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out ReportResultDto? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            // Create filter from report request
            var filter = CreateTransactionFilter(request);

            // Get filtered transactions and summary
            var transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
            var summary = await _transactionService.GetTransactionSummaryAsync(userId, filter);

            // Create result
            var result = new ReportResultDto
            {
                ReportRequest = request,
                Transactions = transactions.ToList(),
                Summary = summary,
                GeneratedDate = DateTime.UtcNow,
                TotalTransactions = transactions.Count(),
                DateRange = $"{(request.StartDate?.ToString("dd/MM/yyyy") ?? "Tüm geçmiş")} - {(request.EndDate?.ToString("dd/MM/yyyy") ?? "Bugüne kadar")}"
            };

            // Get additional analytics
            result.CategoryBreakdown = await GetCategoryBreakdownAsync(request, userId);
            result.AccountBreakdown = await GetAccountBreakdownAsync(request, userId);
            result.MonthlyTrends = await GetMonthlyTrendsAsync(request, userId);

            // Cache the result
            _cache.Set(cacheKey, result, _cacheExpiration);

            return result;
        }
        catch (Exception)
        {
            return new ReportResultDto
            {
                ReportRequest = request,
                ErrorMessage = "Rapor oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.",
                GeneratedDate = DateTime.UtcNow,
                Transactions = new List<TransactionDto>(),
                Summary = new TransactionSummaryDto()
            };
        }
    }

    public async Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(ReportRequestDto request, string userId)
    {
        var filter = CreateTransactionFilter(request);
        var transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
        
        var totalAmount = transactions.Sum(t => Math.Abs(t.Amount));
        
        return transactions
            .Where(t => t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new CategoryBreakdownDto
            {
                CategoryId = g.Key,
                CategoryName = _simpleCategoryService.GetCategoryName(g.Key),
                TotalAmount = g.Sum(t => Math.Abs(t.Amount)),
                TransactionCount = g.Count(),
                Percentage = totalAmount > 0 ? (g.Sum(t => Math.Abs(t.Amount)) / totalAmount) * 100 : 0,
                Color = GetCategoryColor(g.Key)
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }

    public async Task<List<AccountBreakdownDto>> GetAccountBreakdownAsync(ReportRequestDto request, string userId)
    {
        var filter = CreateTransactionFilter(request);
        var transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
        
        return transactions
            .Where(t => t.Account != null)
            .GroupBy(t => new { t.AccountId, t.Account!.Name })
            .Select(g => new AccountBreakdownDto
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                IncomeTotal = g.Where(t => t.Type == Models.TransactionType.Income).Sum(t => t.Amount),
                ExpenseTotal = g.Where(t => t.Type == Models.TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderByDescending(a => Math.Abs(a.TotalAmount))
            .ToList();
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendsAsync(ReportRequestDto request, string userId)
    {
        var filter = CreateTransactionFilter(request);
        var transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
        
        return transactions
            .GroupBy(t => new { Year = t.TransactionDate.Year, Month = t.TransactionDate.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                MonthYear = $"{g.Key.Month:00}/{g.Key.Year}",
                IncomeTotal = g.Where(t => t.Type == Models.TransactionType.Income).Sum(t => t.Amount),
                ExpenseTotal = g.Where(t => t.Type == Models.TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderBy(m => m.Month)
            .ToList();
    }

    public async Task<byte[]> ExportToPdfAsync(ReportResultDto reportResult)
    {
        // TODO: Implement PDF export using PuppeteerSharp or similar
        // This is a placeholder implementation
        await Task.Delay(100);
        return Array.Empty<byte>();
    }

    public (DateTime startDate, DateTime endDate) GetDateRangeFromPreset(DateRangePreset preset)
    {
        var today = DateTime.Today;
        var utcToday = DateTime.UtcNow.Date;
        
        return preset switch
        {
            DateRangePreset.Today => (utcToday, utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.Yesterday => (utcToday.AddDays(-1), utcToday.AddTicks(-1)),
            DateRangePreset.ThisWeek => (utcToday.AddDays(-(int)utcToday.DayOfWeek + 1), utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.LastWeek => (utcToday.AddDays(-(int)utcToday.DayOfWeek - 6), utcToday.AddDays(-(int)utcToday.DayOfWeek)),
            DateRangePreset.ThisMonth => (new DateTime(utcToday.Year, utcToday.Month, 1), utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.LastMonth => (new DateTime(utcToday.Year, utcToday.Month, 1).AddMonths(-1), new DateTime(utcToday.Year, utcToday.Month, 1).AddTicks(-1)),
            DateRangePreset.Last3Months => (utcToday.AddMonths(-3), utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.Last6Months => (utcToday.AddMonths(-6), utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.ThisYear => (new DateTime(utcToday.Year, 1, 1), utcToday.AddDays(1).AddTicks(-1)),
            DateRangePreset.LastYear => (new DateTime(utcToday.Year - 1, 1, 1), new DateTime(utcToday.Year, 1, 1).AddTicks(-1)),
            _ => (utcToday.AddMonths(-1), utcToday.AddDays(1).AddTicks(-1))
        };
    }

    public List<string> ValidateReportRequest(ReportRequestDto request)
    {
        var errors = new List<string>();

        // Only validate dates if both are provided
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            if (request.StartDate > request.EndDate)
            {
                errors.Add("Başlangıç tarihi bitiş tarihinden büyük olamaz.");
            }

            if ((request.EndDate.Value - request.StartDate.Value).TotalDays > 365 * 2)
            {
                errors.Add("Rapor dönemi en fazla 2 yıl olabilir.");
            }
        }

        // Validate individual dates if provided
        if (request.StartDate.HasValue && request.StartDate > DateTime.UtcNow)
        {
            errors.Add("Başlangıç tarihi gelecekte olamaz.");
        }

        if (request.EndDate.HasValue && request.EndDate < DateTime.UtcNow.AddYears(-10))
        {
            errors.Add("Bitiş tarihi çok eski bir tarih olamaz.");
        }

        if (request.MinAmount.HasValue && request.MaxAmount.HasValue && request.MinAmount > request.MaxAmount)
        {
            errors.Add("Minimum tutar maksimum tutardan büyük olamaz.");
        }

        if (!string.IsNullOrEmpty(request.SearchTerm) && request.SearchTerm.Length < 2)
        {
            errors.Add("Arama terimi en az 2 karakter olmalıdır.");
        }

        return errors;
    }

    private TransactionFilterDto CreateTransactionFilter(ReportRequestDto request)
    {
        return new TransactionFilterDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Type = request.TransactionType,
            IsRecurring = request.IncludeRecurring ? null : false,
            Search = request.SearchTerm
        };
    }

    private string GetCategoryColor(int categoryId)
    {
        // Generate consistent colors for categories
        var colors = new[]
        {
            "#007bff", "#28a745", "#dc3545", "#ffc107", "#6f42c1",
            "#fd7e14", "#20c997", "#6610f2", "#e83e8c", "#17a2b8"
        };
        
        return colors[categoryId % colors.Length];
    }

    private string GenerateCacheKey(ReportRequestDto request, string userId)
    {
        var key = $"report_{userId}_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}_" +
                  $"{request.AccountId}_{request.CategoryId}_{request.TransactionType}_" +
                  $"{request.IncludeRecurring}_{request.IncludeInstallments}_" +
                  $"{request.MinAmount}_{request.MaxAmount}_{request.SearchTerm}";
        return key;
    }
}