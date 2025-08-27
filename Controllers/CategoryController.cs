using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public IActionResult Index()
    {
        // Categories are now managed through simple predefined list
        return RedirectToAction("Index", "Transaction");
    }

    [HttpGet]
    public IActionResult Create()
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateCategoryDto model)
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UpdateCategoryDto model)
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }

    public IActionResult Details(int id)
    {
        TempData["Error"] = "Category management is no longer available. Categories are now predefined.";
        return RedirectToAction("Index", "Transaction");
    }
}