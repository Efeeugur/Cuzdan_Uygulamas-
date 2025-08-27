using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.DataAccess.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetAccountsByUserIdAsync(string userId);
    Task<Account?> GetAccountWithTransactionsAsync(int accountId);
    Task<decimal> GetTotalBalanceByUserIdAsync(string userId);
}