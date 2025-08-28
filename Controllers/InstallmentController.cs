using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.Controllers;

[Authorize]
public class InstallmentController : Controller
{
    private readonly IInstallmentService _installmentService;
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;

    public InstallmentController(
        IInstallmentService installmentService,
        ICategoryService categoryService,
        IAccountService accountService)
    {
        _installmentService = installmentService;
        _categoryService = categoryService;
        _accountService = accountService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var installments = await _installmentService.GetInstallmentsByUserIdAsync(userId);
        return View(installments);
    }

    public async Task<IActionResult> Active()
    {
        var userId = GetUserId();
        var activeInstallments = await _installmentService.GetActiveInstallmentsAsync(userId);
        return View(activeInstallments);
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
    public async Task<IActionResult> Create(CreateInstallmentDto model)
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
            await _installmentService.CreateInstallmentAsync(model, userId);
            TempData["Success"] = "Installment created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Add detailed error logging for debugging
            var innerException = ex.InnerException?.Message ?? "No inner exception";
            var fullError = $"Taksit kaydedilirken bir hata oluştu: {ex.Message}. Inner Exception: {innerException}";
            
            // Log the full exception for debugging
            Console.WriteLine($"Installment Creation Error: {ex}");
            
            ModelState.AddModelError(string.Empty, fullError);
            var userId = GetUserId();
            await PopulateDropdownsAsync(userId);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var installment = await _installmentService.GetInstallmentByIdAsync(id, userId);

        if (installment == null)
            return NotFound();

        var model = new UpdateInstallmentDto
        {
            Id = installment.Id,
            Description = installment.Description,
            InterestRate = installment.InterestRate,
            NextPaymentDate = installment.NextPaymentDate,
            CategoryId = installment.CategoryId
        };

        await PopulateDropdownsAsync(userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateInstallmentDto model)
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
            var result = await _installmentService.UpdateInstallmentAsync(model, userId);

            if (result == null)
                return NotFound();

            TempData["Success"] = "Installment updated successfully.";
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
            var result = await _installmentService.DeleteInstallmentAsync(id, userId);

            if (!result)
                return NotFound();

            TempData["Success"] = "Installment deleted successfully.";
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
        var installment = await _installmentService.GetInstallmentWithTransactionsAsync(id, userId);

        if (installment == null)
            return NotFound();

        return View(installment);
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int id)
    {
        var userId = GetUserId();
        var installment = await _installmentService.GetInstallmentByIdAsync(id, userId);

        if (installment == null || installment.IsCompleted)
            return NotFound();

        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
        ViewBag.Accounts = new SelectList(accounts, "Id", "Name");
        ViewBag.InstallmentId = id;
        ViewBag.PaymentAmount = installment.MonthlyPayment;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int installmentId, int accountId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _installmentService.ProcessInstallmentPaymentAsync(installmentId, accountId, userId);

            if (!result)
                return NotFound();

            TempData["Success"] = "Installment payment processed successfully.";
            return RedirectToAction(nameof(Details), new { id = installmentId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Pay), new { id = installmentId });
        }
    }

    private async Task PopulateDropdownsAsync(string userId)
    {
        // Populate categories (both database and SimpleCategoryService categories)
        var dbCategories = await _categoryService.GetCategoriesByUserIdAsync(userId);
        var categoryList = new List<SelectListItem>();
        
        // Add database categories
        foreach (var category in dbCategories)
        {
            categoryList.Add(new SelectListItem 
            { 
                Value = category.Id.ToString(), 
                Text = category.Name 
            });
        }
        
        // Add SimpleCategoryService installment categories (26-35)
        var installmentCategoryNames = new Dictionary<int, string>
        {
            { 26, "Elektronik Taksiti" },
            { 27, "Mobilya Taksiti" },
            { 28, "Beyaz Eşya Taksiti" },
            { 29, "Otomobil Taksiti" },
            { 30, "Kredi Kartı Taksiti" },
            { 31, "Ev Eşyası Taksiti" },
            { 32, "Teknoloji Taksiti" },
            { 33, "Giyim Taksiti" },
            { 34, "Eğitim Taksiti" },
            { 35, "Diğer Taksitler" }
        };
        
        foreach (var installmentCategory in installmentCategoryNames)
        {
            categoryList.Add(new SelectListItem 
            { 
                Value = installmentCategory.Key.ToString(), 
                Text = installmentCategory.Value
            });
        }
        
        ViewBag.Categories = new SelectList(categoryList, "Value", "Text");
    }
}