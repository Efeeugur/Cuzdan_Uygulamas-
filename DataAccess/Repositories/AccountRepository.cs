using Microsoft.EntityFrameworkCore;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.DataAccess.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context, ILogger<Repository<Account>> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .Include(a => a.Transactions) // Include transactions for proper calculation
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetAccountWithTransactionsAsync(int accountId)
    {
        return await _dbSet
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<decimal> GetTotalBalanceByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .SumAsync(a => a.Balance);
    }
}