using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface IPdfService
{
    /// <summary>
    /// Generates a PDF report from the provided report data
    /// </summary>
    /// <param name="reportResult">Report data to convert to PDF</param>
    /// <param name="options">PDF generation options</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateReportPdfAsync(ReportResultDto reportResult, PdfExportOptions? options = null);

    /// <summary>
    /// Generates a PDF from HTML content
    /// </summary>
    /// <param name="html">HTML content to convert</param>
    /// <param name="options">PDF generation options</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GeneratePdfFromHtmlAsync(string html, PdfExportOptions? options = null);

    /// <summary>
    /// Gets the HTML template for PDF generation
    /// </summary>
    /// <param name="reportResult">Report data</param>
    /// <param name="includeCharts">Whether to include chart images</param>
    /// <returns>HTML string ready for PDF conversion</returns>
    string GetReportHtml(ReportResultDto reportResult, bool includeCharts = true);
}

public class PdfExportOptions
{
    public string Format { get; set; } = "A4";
    public bool Landscape { get; set; } = false;
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeTransactionDetails { get; set; } = true;
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public double MarginTop { get; set; } = 1.0;
    public double MarginBottom { get; set; } = 1.0;
    public double MarginLeft { get; set; } = 1.0;
    public double MarginRight { get; set; } = 1.0;
    public double Scale { get; set; } = 0.8;
}