using Cüzdan_Uygulaması.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public interface ISimpleCategoryService
{
    IEnumerable<SelectListItem> GetIncomeCategories();
    IEnumerable<SelectListItem> GetExpenseCategories();
    IEnumerable<SelectListItem> GetAllCategories();
    string GetCategoryName(int categoryId);
}

public class SimpleCategoryService : ISimpleCategoryService
{
    private static readonly Dictionary<int, (string Name, CategoryType Type)> Categories = new()
    {
        // Income Categories (1-10)
        { 1, ("Maaş", CategoryType.Income) },
        { 2, ("Prim", CategoryType.Income) },
        { 3, ("Freelance", CategoryType.Income) },
        { 4, ("Yatırım", CategoryType.Income) },
        { 5, ("Satış", CategoryType.Income) },

        // Expense Categories (11-25)
        { 11, ("Market", CategoryType.Expense) },
        { 12, ("Ulaşım", CategoryType.Expense) },
        { 13, ("Faturalar", CategoryType.Expense) },
        { 14, ("Kira", CategoryType.Expense) },
        { 15, ("Eğlence", CategoryType.Expense) },
        { 16, ("Sağlık", CategoryType.Expense) },
        { 17, ("Giyim", CategoryType.Expense) },
        { 18, ("Eğitim", CategoryType.Expense) },
        { 19, ("Restoran", CategoryType.Expense) },
        { 20, ("Diğer", CategoryType.Expense) }
    };

    public IEnumerable<SelectListItem> GetIncomeCategories()
    {
        return Categories.Where(c => c.Value.Type == CategoryType.Income)
                        .Select(c => new SelectListItem 
                        { 
                            Value = c.Key.ToString(), 
                            Text = c.Value.Name 
                        });
    }

    public IEnumerable<SelectListItem> GetExpenseCategories()
    {
        return Categories.Where(c => c.Value.Type == CategoryType.Expense)
                        .Select(c => new SelectListItem 
                        { 
                            Value = c.Key.ToString(), 
                            Text = c.Value.Name 
                        });
    }

    public IEnumerable<SelectListItem> GetAllCategories()
    {
        return Categories.Select(c => new SelectListItem 
        { 
            Value = c.Key.ToString(), 
            Text = c.Value.Name,
            Group = new SelectListGroup { Name = c.Value.Type == CategoryType.Income ? "Gelir" : "Gider" }
        });
    }

    public string GetCategoryName(int categoryId)
    {
        return Categories.TryGetValue(categoryId, out var category) ? category.Name : "Bilinmiyor";
    }
}