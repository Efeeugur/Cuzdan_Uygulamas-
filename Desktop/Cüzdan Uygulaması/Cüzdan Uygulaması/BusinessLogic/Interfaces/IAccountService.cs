using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAccountsByUserIdAsync(string userId);
    Task<AccountDto?> GetAccountByIdAsync(int id, string userId);
    Task<AccountDto?> GetAccountWithTransactionsAsync(int id, string userId);
    Task<decimal> GetTotalBalanceByUserIdAsync(string userId);
    Task<AccountDto> CreateAccountAsync(CreateAccountDto createAccountDto, string userId);
    Task<AccountDto?> UpdateAccountAsync(UpdateAccountDto updateAccountDto, string userId);
    Task<bool> DeleteAccountAsync(int id, string userId);
    Task<bool> UpdateAccountBalanceAsync(int accountId, decimal amount, string userId);
}