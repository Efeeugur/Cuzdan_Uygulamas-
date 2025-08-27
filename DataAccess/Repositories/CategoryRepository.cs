using Microsoft.EntityFrameworkCore;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.DataAccess.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryWithTransactionsAsync(int categoryId)
    {
        return await _dbSet
            .Include(c => c.Transactions)
                .ThenInclude(t => t.Account)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }
}