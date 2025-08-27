using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Mappers;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AccountDto>> GetAccountsByUserIdAsync(string userId)
    {
        var accounts = await _unitOfWork.Accounts.GetAccountsByUserIdAsync(userId);
        return accounts.Select(a => a.ToDto());
    }

    public async Task<AccountDto?> GetAccountByIdAsync(int id, string userId)
    {
        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        return account?.ToDto();
    }

    public async Task<AccountDto?> GetAccountWithTransactionsAsync(int id, string userId)
    {
        var account = await _unitOfWork.Accounts.GetAccountWithTransactionsAsync(id);
        
        if (account == null || account.UserId != userId)
            return null;

        return account.ToDto();
    }

    public async Task<decimal> GetTotalBalanceByUserIdAsync(string userId)
    {
        return await _unitOfWork.Accounts.GetTotalBalanceByUserIdAsync(userId);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto createAccountDto, string userId)
    {
        if (string.IsNullOrWhiteSpace(createAccountDto.Name))
            throw new ArgumentException("Account name is required.");

        var existingAccount = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.UserId == userId && a.Name.ToLower() == createAccountDto.Name.ToLower());

        if (existingAccount != null)
            throw new InvalidOperationException("An account with this name already exists.");

        var account = createAccountDto.ToEntity(userId);
        await _unitOfWork.Accounts.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return account.ToDto();
    }

    public async Task<AccountDto?> UpdateAccountAsync(UpdateAccountDto updateAccountDto, string userId)
    {
        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == updateAccountDto.Id && a.UserId == userId);

        if (account == null)
            return null;

        if (string.IsNullOrWhiteSpace(updateAccountDto.Name))
            throw new ArgumentException("Account name is required.");

        var existingAccount = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.UserId == userId && 
                 a.Name.ToLower() == updateAccountDto.Name.ToLower() && 
                 a.Id != updateAccountDto.Id);

        if (existingAccount != null)
            throw new InvalidOperationException("An account with this name already exists.");

        account.Name = updateAccountDto.Name;
        account.Description = updateAccountDto.Description;

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return account.ToDto();
    }

    public async Task<bool> DeleteAccountAsync(int id, string userId)
    {
        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
            return false;

        var hasTransactions = await _unitOfWork.Transactions.AnyAsync(t => t.AccountId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete account with existing transactions.");

        _unitOfWork.Accounts.Remove(account);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAccountBalanceAsync(int accountId, decimal amount, string userId)
    {
        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == accountId && a.UserId == userId);

        if (account == null)
            return false;

        account.Balance += amount;
        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}