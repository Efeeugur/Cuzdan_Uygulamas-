using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Generates a comprehensive report based on the provided request criteria
    /// </summary>
    /// <param name="request">Report request with filtering criteria</param>
    /// <param name="userId">User ID for data filtering</param>
    /// <returns>Complete report result with data and analytics</returns>
    Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId);

    /// <summary>
    /// Gets category breakdown data for pie chart visualization
    /// </summary>
    /// <param name="request">Report request with filtering criteria</param>
    /// <param name="userId">User ID for data filtering</param>
    /// <returns>List of category breakdown data</returns>
    Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(ReportRequestDto request, string userId);

    /// <summary>
    /// Gets account breakdown data for account analysis
    /// </summary>
    /// <param name="request">Report request with filtering criteria</param>
    /// <param name="userId">User ID for data filtering</param>
    /// <returns>List of account breakdown data</returns>
    Task<List<AccountBreakdownDto>> GetAccountBreakdownAsync(ReportRequestDto request, string userId);

    /// <summary>
    /// Gets monthly trend data for line chart visualization
    /// </summary>
    /// <param name="request">Report request with filtering criteria</param>
    /// <param name="userId">User ID for data filtering</param>
    /// <returns>List of monthly trend data</returns>
    Task<List<MonthlyTrendDto>> GetMonthlyTrendsAsync(ReportRequestDto request, string userId);

    /// <summary>
    /// Exports report data to PDF format
    /// </summary>
    /// <param name="reportResult">Report data to export</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> ExportToPdfAsync(ReportResultDto reportResult);

    /// <summary>
    /// Gets predefined date range based on preset selection
    /// </summary>
    /// <param name="preset">Date range preset type</param>
    /// <returns>Start and end dates for the preset</returns>
    (DateTime startDate, DateTime endDate) GetDateRangeFromPreset(DateRangePreset preset);

    /// <summary>
    /// Validates report request parameters
    /// </summary>
    /// <param name="request">Report request to validate</param>
    /// <returns>List of validation error messages</returns>
    List<string> ValidateReportRequest(ReportRequestDto request);
}