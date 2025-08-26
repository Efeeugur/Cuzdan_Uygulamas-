using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.DataAccess.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(string userId);
    Task<Category?> GetCategoryWithTransactionsAsync(int categoryId);
}