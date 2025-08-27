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
public class TransactionController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ISimpleCategoryService _simpleCategoryService;
    private readonly IInstallmentService _installmentService;

    public TransactionController(
        ITransactionService transactionService,
        IAccountService accountService,
        ISimpleCategoryService simpleCategoryService,
        IInstallmentService installmentService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _simpleCategoryService = simpleCategoryService;
        _installmentService = installmentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(TransactionFilterDto? filter)
    {
        var userId = GetUserId();
        IEnumerable<TransactionDto> transactions;
        TransactionSummaryDto summary;

        if (filter != null && HasAnyFilter(filter))
        {
            transactions = await _transactionService.GetFilteredTransactionsAsync(userId, filter);
            summary = await _transactionService.GetTransactionSummaryAsync(userId, filter);
        }
        else
        {
            transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
            summary = await _transactionService.GetTransactionSummaryAsync(userId);
        }

        ViewBag.Summary = summary;
        ViewBag.Filter = filter ?? new TransactionFilterDto();

        // Populate filter dropdowns
        await PopulateFilterDropdownsAsync(userId);

        return View(transactions);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = GetUserId();
        await PopulateDropdownsAsync(userId);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTransactionDto model)
    {
        if (!ModelState.IsValid)
        {
            var userId = GetUserId();
            await PopulateDropdownsAsync(userId);
            return View(model);
        }

        try
        {
            var userId = GetUserId();
            await _transactionService.CreateTransactionAsync(model, userId);
            TempData["Success"] = "Transaction created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var userId = GetUserId();
            await PopulateDropdownsAsync(userId);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
            return NotFound();

        var model = new UpdateTransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            Type = transaction.Type,
            IsRecurring = transaction.IsRecurring,
            RecurrenceType = transaction.RecurrenceType,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            InstallmentId = transaction.InstallmentId
        };

        await PopulateDropdownsAsync(userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTransactionDto model)
    {
        if (!ModelState.IsValid)
        {
            var userId = GetUserId();
            await PopulateDropdownsAsync(userId);
            return View(model);
        }

        try
        {
            var userId = GetUserId();
            var result = await _transactionService.UpdateTransactionAsync(model, userId);

            if (result == null)
                return NotFound();

            TempData["Success"] = "Transaction updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var userId = GetUserId();
            await PopulateDropdownsAsync(userId);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _transactionService.DeleteTransactionAsync(id, userId);

            if (!result)
                return NotFound();

            TempData["Success"] = "Transaction deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
            return NotFound();

        return View(transaction);
    }

    private async Task PopulateDropdownsAsync(string userId)
    {
        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
        var installments = await _installmentService.GetActiveInstallmentsAsync(userId);

        ViewBag.Accounts = new SelectList(accounts, "Id", "Name");
        ViewBag.Categories = _simpleCategoryService.GetAllCategories();
        ViewBag.Installments = new SelectList(installments, "Id", "Description");
        ViewBag.TransactionTypes = new SelectList(Enum.GetValues<TransactionType>());
        ViewBag.RecurrenceTypes = new SelectList(Enum.GetValues<RecurrenceType>());
    }

    private async Task PopulateFilterDropdownsAsync(string userId)
    {
        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);

        ViewBag.FilterAccounts = new SelectList(accounts, "Id", "Name");
        ViewBag.FilterCategories = _simpleCategoryService.GetAllCategories();
        ViewBag.FilterTransactionTypes = new SelectList(new[]
        {
            new { Value = "", Text = "All Types" },
            new { Value = ((int)TransactionType.Income).ToString(), Text = "Income" },
            new { Value = ((int)TransactionType.Expense).ToString(), Text = "Expense" }
        }, "Value", "Text");
        ViewBag.FilterRecurringOptions = new SelectList(new[]
        {
            new { Value = "", Text = "All Transactions" },
            new { Value = "true", Text = "Recurring Only" },
            new { Value = "false", Text = "One-time Only" }
        }, "Value", "Text");
    }

    private static bool HasAnyFilter(TransactionFilterDto filter)
    {
        return filter.StartDate.HasValue ||
               filter.EndDate.HasValue ||
               filter.AccountId.HasValue ||
               filter.CategoryId.HasValue ||
               filter.Type.HasValue ||
               filter.IsRecurring.HasValue ||
               !string.IsNullOrWhiteSpace(filter.Search);
    }
}