using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;

namespace Cüzdan_Uygulaması.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IInstallmentService _installmentService;

    public HomeController(
        ILogger<HomeController> logger,
        IAccountService accountService,
        ITransactionService transactionService,
        IInstallmentService installmentService)
    {
        _logger = logger;
        _accountService = accountService;
        _transactionService = transactionService;
        _installmentService = installmentService;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity!.IsAuthenticated)
        {
            return View();
        }


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        try
        {
            var totalBalance = await _accountService.GetTotalBalanceByUserIdAsync(userId);
            var recentTransactions = (await _transactionService.GetTransactionsByUserIdAsync(userId)).Take(5);
            var upcomingInstallments = (await _installmentService.GetActiveInstallmentsAsync(userId))
                .Where(i => i.NextPaymentDate <= DateTime.UtcNow.AddDays(7))
                .Take(5);
            var transactionSummary = await _transactionService.GetTransactionSummaryAsync(userId);

            ViewBag.TotalBalance = totalBalance;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.UpcomingInstallments = upcomingInstallments;
            ViewBag.TransactionSummary = transactionSummary;
        }
        catch
        {
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
