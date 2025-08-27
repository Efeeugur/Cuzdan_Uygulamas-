using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.DataAccess.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(string userId);
    Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId);
    Task<IEnumerable<Transaction>> GetTransactionsByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync();
    Task<decimal> GetTotalIncomeByUserIdAsync(string userId);
    Task<decimal> GetTotalExpenseByUserIdAsync(string userId);
    Task<IEnumerable<Transaction>> GetTransactionsWithDetailsAsync(string userId);
}