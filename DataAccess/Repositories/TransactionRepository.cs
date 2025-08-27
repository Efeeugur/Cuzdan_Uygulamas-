using Microsoft.EntityFrameworkCore;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.DataAccess.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Installment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
    {
        return await _dbSet
            .Where(t => t.AccountId == accountId)
            .Include(t => t.Category)
            .Include(t => t.Installment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Where(t => t.CategoryId == categoryId)
            .Include(t => t.Account)
            .Include(t => t.Installment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && 
                       t.TransactionDate >= startDate && 
                       t.TransactionDate <= endDate)
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Installment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync()
    {
        return await _dbSet
            .Where(t => t.IsRecurring && t.NextRecurrenceDate <= DateTime.UtcNow)
            .Include(t => t.Account)
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalIncomeByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);
    }

    public async Task<decimal> GetTotalExpenseByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsWithDetailsAsync(string userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Installment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}