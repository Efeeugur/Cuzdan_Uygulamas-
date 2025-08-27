using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public interface IDefaultDataService
{
    Task CreateDefaultCategoriesAsync(string userId);
}

public class DefaultDataService : IDefaultDataService
{
    private readonly ICategoryService _categoryService;

    public DefaultDataService(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task CreateDefaultCategoriesAsync(string userId)
    {
        // Check if user already has categories
        var existingCategories = await _categoryService.GetCategoriesByUserIdAsync(userId);
        if (existingCategories.Any())
            return;

        // Default Income Categories
        var incomeCategories = new[]
        {
            new CreateCategoryDto { Name = "Maaş", Description = "Aylık maaş geliri", Color = "#28a745", Type = CategoryType.Income },
            new CreateCategoryDto { Name = "Prim", Description = "Performans primleri", Color = "#17a2b8", Type = CategoryType.Income },
            new CreateCategoryDto { Name = "Freelance", Description = "Serbest çalışma gelirleri", Color = "#6f42c1", Type = CategoryType.Income },
            new CreateCategoryDto { Name = "Yatırım", Description = "Yatırım gelirleri", Color = "#fd7e14", Type = CategoryType.Income },
            new CreateCategoryDto { Name = "Satış", Description = "Mal/hizmet satış gelirleri", Color = "#20c997", Type = CategoryType.Income }
        };

        // Default Expense Categories
        var expenseCategories = new[]
        {
            new CreateCategoryDto { Name = "Market", Description = "Gıda ve ev ihtiyaçları", Color = "#dc3545", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Ulaşım", Description = "Toplu taşıma, yakıt, taksi", Color = "#6c757d", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Faturalar", Description = "Elektrik, su, gaz, internet", Color = "#e83e8c", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Kira", Description = "Ev kirası", Color = "#6610f2", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Eğlence", Description = "Sinema, restoran, sosyal aktiviteler", Color = "#fd7e14", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Sağlık", Description = "Doktor, ilaç, sağlık giderleri", Color = "#198754", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Giyim", Description = "Kıyafet ve ayakkabı", Color = "#0dcaf0", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Eğitim", Description = "Kitap, kurs, eğitim materyalleri", Color = "#664d03", Type = CategoryType.Expense },
            new CreateCategoryDto { Name = "Diğer", Description = "Sınıflandırılamayan giderler", Color = "#adb5bd", Type = CategoryType.Expense }
        };

        // Create all categories
        try
        {
            foreach (var category in incomeCategories)
            {
                await _categoryService.CreateCategoryAsync(category, userId);
            }

            foreach (var category in expenseCategories)
            {
                await _categoryService.CreateCategoryAsync(category, userId);
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail user registration
            // In production, you would use a proper logging framework
            Console.WriteLine($"Failed to create default categories for user {userId}: {ex.Message}");
        }
    }
}