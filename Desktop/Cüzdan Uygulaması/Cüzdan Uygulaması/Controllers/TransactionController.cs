using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.Controllers;

[Authorize]
public class TransactionController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ICategoryService _categoryService;
    private readonly IInstallmentService _installmentService;

    public TransactionController(
        ITransactionService transactionService,
        IAccountService accountService,
        ICategoryService categoryService,
        IInstallmentService installmentService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
        _installmentService = installmentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
        var summary = await _transactionService.GetTransactionSummaryAsync(userId);

        ViewBag.Summary = summary;
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
        var categories = await _categoryService.GetCategoriesByUserIdAsync(userId);
        var installments = await _installmentService.GetActiveInstallmentsAsync(userId);

        ViewBag.Accounts = new SelectList(accounts, "Id", "Name");
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Installments = new SelectList(installments, "Id", "Description");
        ViewBag.TransactionTypes = new SelectList(Enum.GetValues<TransactionType>());
        ViewBag.RecurrenceTypes = new SelectList(Enum.GetValues<RecurrenceType>());
    }
}