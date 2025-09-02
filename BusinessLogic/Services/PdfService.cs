using PuppeteerSharp;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using System.Text;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class PdfService : IPdfService
{
    private readonly ISimpleCategoryService _simpleCategoryService;
    private static bool _browserInitialized = false;

    public PdfService(ISimpleCategoryService simpleCategoryService)
    {
        _simpleCategoryService = simpleCategoryService;
    }

    public async Task<byte[]> GenerateReportPdfAsync(ReportResultDto reportResult, PdfExportOptions? options = null)
    {
        options ??= new PdfExportOptions();
        
        var html = GetReportHtml(reportResult, options.IncludeCharts);
        return await GeneratePdfFromHtmlAsync(html, options);
    }

    public async Task<byte[]> GeneratePdfFromHtmlAsync(string html, PdfExportOptions? options = null)
    {
        options ??= new PdfExportOptions();

        try
        {
            await EnsureBrowserInitializedAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            using var page = await browser.NewPageAsync();
            
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                Format = options.Format == "A4" ? PuppeteerSharp.Media.PaperFormat.A4 : PuppeteerSharp.Media.PaperFormat.Letter,
                Landscape = options.Landscape,
                PrintBackground = true,
                MarginOptions = new PuppeteerSharp.Media.MarginOptions
                {
                    Top = $"{options.MarginTop}in",
                    Bottom = $"{options.MarginBottom}in",
                    Left = $"{options.MarginLeft}in",
                    Right = $"{options.MarginRight}in"
                },
                Scale = (decimal)options.Scale,
                HeaderTemplate = !string.IsNullOrEmpty(options.HeaderText) 
                    ? $"<div style='font-size: 10px; width: 100%; text-align: center;'>{options.HeaderText}</div>"
                    : "",
                FooterTemplate = !string.IsNullOrEmpty(options.FooterText) 
                    ? $"<div style='font-size: 10px; width: 100%; text-align: center;'>{options.FooterText} - Sayfa <span class='pageNumber'></span> / <span class='totalPages'></span></div>"
                    : "<div style='font-size: 10px; width: 100%; text-align: center;'>Sayfa <span class='pageNumber'></span> / <span class='totalPages'></span></div>",
                DisplayHeaderFooter = true
            };

            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"PDF oluşturulurken hata oluştu: {ex.Message}", ex);
        }
    }

    public string GetReportHtml(ReportResultDto reportResult, bool includeCharts = true)
    {
        var html = new StringBuilder();
        
        html.Append(@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Finansal Rapor</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12px;
            line-height: 1.6;
            color: #333;
            background: white;
        }

        .container {
            max-width: 100%;
            margin: 0 auto;
            padding: 20px;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 3px solid #007aff;
        }

        .header h1 {
            color: #007aff;
            font-size: 28px;
            margin-bottom: 10px;
        }

        .header .subtitle {
            color: #666;
            font-size: 14px;
        }

        .summary-section {
            margin-bottom: 30px;
        }

        .summary-cards {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 15px;
            margin-bottom: 20px;
        }

        .summary-card {
            background: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
        }

        .summary-card .value {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 5px;
        }

        .summary-card .label {
            font-size: 11px;
            color: #666;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .income { color: #28a745; }
        .expense { color: #dc3545; }
        .net.positive { color: #28a745; }
        .net.negative { color: #dc3545; }

        .section {
            margin-bottom: 30px;
        }

        .section h2 {
            color: #007aff;
            font-size: 18px;
            margin-bottom: 15px;
            padding-bottom: 5px;
            border-bottom: 2px solid #e9ecef;
        }

        .breakdown-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 20px;
        }

        .breakdown-item {
            background: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 6px;
            padding: 10px;
        }

        .breakdown-item .name {
            font-weight: bold;
            margin-bottom: 5px;
        }

        .breakdown-item .amount {
            font-size: 14px;
            font-weight: bold;
        }

        .breakdown-item .details {
            font-size: 10px;
            color: #666;
            margin-top: 3px;
        }

        .transactions-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
            font-size: 11px;
        }

        .transactions-table th {
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 8px 6px;
            font-weight: bold;
            text-align: left;
        }

        .transactions-table td {
            border: 1px solid #dee2e6;
            padding: 6px;
        }

        .transactions-table .amount {
            text-align: right;
            font-weight: bold;
        }

        .transactions-table .income-amount {
            color: #28a745;
        }

        .transactions-table .expense-amount {
            color: #dc3545;
        }

        .page-break {
            page-break-before: always;
        }

        .footer-info {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e9ecef;
            font-size: 10px;
            color: #666;
            text-align: center;
        }

        @media print {
            body { print-color-adjust: exact; }
            .page-break { page-break-before: always; }
        }
    </style>
</head>
<body>
    <div class='container'>");

        // Header
        html.Append($@"
        <div class='header'>
            <h1>📊 Finansal Rapor</h1>
            <div class='subtitle'>
                {reportResult.DateRange} | Oluşturulma: {reportResult.GeneratedDate:dd/MM/yyyy HH:mm}
            </div>
        </div>");

        // Summary Section
        html.Append(@"
        <div class='summary-section'>
            <h2>Özet Bilgiler</h2>
            <div class='summary-cards'>");
        
        html.Append($@"
                <div class='summary-card'>
                    <div class='value income'>₺{reportResult.Summary.TotalIncome:N2}</div>
                    <div class='label'>Toplam Gelir</div>
                </div>
                <div class='summary-card'>
                    <div class='value expense'>₺{reportResult.Summary.TotalExpense:N2}</div>
                    <div class='label'>Toplam Gider</div>
                </div>
                <div class='summary-card'>
                    <div class='value net {(reportResult.Summary.NetAmount >= 0 ? "positive" : "negative")}'>₺{reportResult.Summary.NetAmount:N2}</div>
                    <div class='label'>Net Durum</div>
                </div>
                <div class='summary-card'>
                    <div class='value'>{reportResult.TotalTransactions}</div>
                    <div class='label'>İşlem Sayısı</div>
                </div>");
        
        html.Append("</div></div>");

        // Category Breakdown
        if (reportResult.CategoryBreakdown.Any())
        {
            html.Append(@"
            <div class='section'>
                <h2>Kategori Analizi</h2>
                <div class='breakdown-grid'>");

            foreach (var category in reportResult.CategoryBreakdown.Take(8))
            {
                html.Append($@"
                    <div class='breakdown-item'>
                        <div class='name'>{category.CategoryName}</div>
                        <div class='amount'>₺{category.TotalAmount:N2}</div>
                        <div class='details'>{category.TransactionCount} işlem • %{category.Percentage:F1}</div>
                    </div>");
            }

            html.Append("</div></div>");
        }

        // Account Breakdown
        if (reportResult.AccountBreakdown.Any())
        {
            html.Append(@"
            <div class='section'>
                <h2>Hesap Analizi</h2>
                <div class='breakdown-grid'>");

            foreach (var account in reportResult.AccountBreakdown.Take(6))
            {
                html.Append($@"
                    <div class='breakdown-item'>
                        <div class='name'>{account.AccountName}</div>
                        <div class='amount {(account.TotalAmount >= 0 ? "income" : "expense")}'>₺{account.TotalAmount:N2}</div>
                        <div class='details'>
                            Gelir: ₺{account.IncomeTotal:N0} | Gider: ₺{Math.Abs(account.ExpenseTotal):N0}
                        </div>
                    </div>");
            }

            html.Append("</div></div>");
        }

        // Transactions Table
        if (reportResult.Transactions.Any())
        {
            html.Append(@"
            <div class='section page-break'>
                <h2>İşlem Detayları</h2>
                <table class='transactions-table'>
                    <thead>
                        <tr>
                            <th>Tarih</th>
                            <th>Açıklama</th>
                            <th>Kategori</th>
                            <th>Hesap</th>
                            <th>Tutar</th>
                            <th>Tür</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var transaction in reportResult.Transactions.OrderByDescending(t => t.TransactionDate).Take(50))
            {
                var categoryName = transaction.CategoryId.HasValue 
                    ? _simpleCategoryService.GetCategoryName(transaction.CategoryId.Value) 
                    : "-";
                
                var amountClass = transaction.Type == TransactionType.Income ? "income-amount" : "expense-amount";
                var amountPrefix = transaction.Type == TransactionType.Income ? "+" : "-";
                var typeText = transaction.Type == TransactionType.Income ? "Gelir" : "Gider";

                html.Append($@"
                        <tr>
                            <td>{transaction.TransactionDate:dd/MM/yyyy}</td>
                            <td>{transaction.Description}</td>
                            <td>{categoryName}</td>
                            <td>{transaction.Account?.Name ?? "-"}</td>
                            <td class='amount {amountClass}'>{amountPrefix}₺{transaction.Amount:N2}</td>
                            <td>{typeText}</td>
                        </tr>");
            }

            html.Append("</tbody></table>");

            if (reportResult.Transactions.Count > 50)
            {
                html.Append($@"
                <div style='margin-top: 10px; font-style: italic; color: #666;'>
                    * İlk 50 işlem gösterilmektedir. Toplam {reportResult.Transactions.Count} işlem bulunmaktadır.
                </div>");
            }

            html.Append("</div>");
        }

        // Footer
        html.Append($@"
        <div class='footer-info'>
            Bu rapor Cüzdan Uygulaması tarafından {reportResult.GeneratedDate:dd MMMM yyyy} tarihinde otomatik olarak oluşturulmuştur.
        </div>
    </div>
</body>
</html>");

        return html.ToString();
    }

    private static async Task EnsureBrowserInitializedAsync()
    {
        if (!_browserInitialized)
        {
            await new BrowserFetcher().DownloadAsync();
            _browserInitialized = true;
        }
    }
}