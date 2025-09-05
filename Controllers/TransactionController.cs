using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Services;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.Exceptions;

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

        // Check if user has any accounts
        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
        if (!accounts.Any())
        {
            TempData["Warning"] = "İşlem oluşturmadan önce en az bir hesap oluşturmalısınız.";
            return RedirectToAction("CreateAccount", "Wallet");
        }

        await PopulateDropdownsAsync(userId);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTransactionDto model)
    {
        var userId = GetUserId();
        
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(userId);
            return View(model);
        }

        await _transactionService.CreateTransactionAsync(model, userId);
        TempData["Success"] = "İşlem başarıyla kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
            throw new ResourceNotFoundException("Transaction", id);

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
        var userId = GetUserId();
        
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(userId);
            return View(model);
        }

        var result = await _transactionService.UpdateTransactionAsync(model, userId);

        if (result == null)
            throw new ResourceNotFoundException("Transaction", model.Id);

        TempData["Success"] = "Transaction updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _transactionService.DeleteTransactionAsync(id, userId);

        if (!result)
            throw new ResourceNotFoundException("Transaction", id);

        TempData["Success"] = "Transaction deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
            throw new ResourceNotFoundException("Transaction", id);

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

    // API endpoints for dynamic category loading
    [HttpGet]
    public IActionResult GetCategoriesByType(string type)
    {
        IEnumerable<SelectListItem> categories = type?.ToLower() switch
        {
            "income" => _simpleCategoryService.GetIncomeCategories(),
            "expense" => _simpleCategoryService.GetExpenseCategories(),
            _ => new List<SelectListItem>()
        };

        var result = categories.Select(c => new { value = c.Value, text = c.Text }).ToArray();
        return Json(result);
    }

    [HttpGet]
    public IActionResult GetAllCategoriesByType()
    {
        var result = _simpleCategoryService.GetCategoriesByType();
        return Json(result);
    }
}