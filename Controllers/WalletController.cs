using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.Logging;

namespace Cüzdan_Uygulaması.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(
        IAccountService accountService,
        ITransactionService transactionService,
        ILogger<WalletController> logger)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        
        using (_logger.BeginScopeWithUserId(userId))
        using (var timer = _logger.TimeOperation("LoadWalletDashboard"))
        {
            var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
            var totalBalance = await _accountService.GetTotalBalanceByUserIdAsync(userId);
            var transactionSummary = await _transactionService.GetTransactionSummaryAsync(userId);

            _logger.LogInformation("Wallet dashboard loaded for user {UserId}: {AccountCount} accounts, {TotalBalance:C} total balance",
                userId, accounts.Count(), totalBalance);

            ViewBag.TotalBalance = totalBalance;
            ViewBag.TransactionSummary = transactionSummary;

            return View(accounts);
        }
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccountDto model)
    {
        var userId = GetUserId();
        
        using (_logger.BeginScopeWithUserId(userId))
        {
            if (!ModelState.IsValid)
            {
                _logger.LogValidationError(userId, "CreateAccount", "Model validation failed");
                return View(model);
            }

            try
            {
                var account = await _accountService.CreateAccountAsync(model, userId);
                _logger.LogAccountCreated(userId, account.Id, account.Name, account.Balance);
                TempData["Success"] = "Account created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account for user {UserId}: {AccountName}",
                    userId, model.Name);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditAccount(int id)
    {
        var userId = GetUserId();
        var account = await _accountService.GetAccountByIdAsync(id, userId);

        if (account == null)
            return NotFound();

        var model = new UpdateAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(UpdateAccountDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var userId = GetUserId();
            var result = await _accountService.UpdateAccountAsync(model, userId);

            if (result == null)
                return NotFound();

            TempData["Success"] = "Account updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _accountService.DeleteAccountAsync(id, userId);

            if (!result)
                return NotFound();

            TempData["Success"] = "Account deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AccountDetails(int id)
    {
        var userId = GetUserId();
        var account = await _accountService.GetAccountWithTransactionsAsync(id, userId);

        if (account == null)
            return NotFound();

        return View(account);
    }
}