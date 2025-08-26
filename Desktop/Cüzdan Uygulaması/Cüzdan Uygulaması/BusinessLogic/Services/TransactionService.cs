using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Mappers;
using Cüzdan_Uygulaması.DataAccess.Interfaces;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(string userId)
    {
        var transactions = await _unitOfWork.Transactions.GetTransactionsWithDetailsAsync(userId);
        return transactions.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByAccountIdAsync(int accountId, string userId)
    {
        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
        if (account == null)
            return Enumerable.Empty<TransactionDto>();

        var transactions = await _unitOfWork.Transactions.GetTransactionsByAccountIdAsync(accountId);
        return transactions.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByCategoryIdAsync(int categoryId, string userId)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
        if (category == null)
            return Enumerable.Empty<TransactionDto>();

        var transactions = await _unitOfWork.Transactions.GetTransactionsByCategoryIdAsync(categoryId);
        return transactions.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.GetTransactionsByDateRangeAsync(userId, startDate, endDate);
        return transactions.Select(t => t.ToDto());
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(int id, string userId)
    {
        var transaction = await _unitOfWork.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        return transaction?.ToDto();
    }

    public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(string userId)
    {
        var totalIncome = await _unitOfWork.Transactions.GetTotalIncomeByUserIdAsync(userId);
        var totalExpense = await _unitOfWork.Transactions.GetTotalExpenseByUserIdAsync(userId);
        var transactionCount = await _unitOfWork.Transactions.CountAsync(t => t.UserId == userId);

        return new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            TransactionCount = transactionCount
        };
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto, string userId)
    {
        if (createTransactionDto.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive.");

        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == createTransactionDto.AccountId && a.UserId == userId);

        if (account == null)
            throw new InvalidOperationException("Account not found or access denied.");

        if (createTransactionDto.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.FirstOrDefaultAsync(
                c => c.Id == createTransactionDto.CategoryId && c.UserId == userId);

            if (category == null)
                throw new InvalidOperationException("Category not found or access denied.");
        }

        if (createTransactionDto.InstallmentId.HasValue)
        {
            var installment = await _unitOfWork.Installments.FirstOrDefaultAsync(
                i => i.Id == createTransactionDto.InstallmentId && i.UserId == userId);

            if (installment == null)
                throw new InvalidOperationException("Installment not found or access denied.");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var transaction = createTransactionDto.ToEntity(userId);
            await _unitOfWork.Transactions.AddAsync(transaction);

            var balanceChange = createTransactionDto.Type == TransactionType.Income 
                ? createTransactionDto.Amount 
                : -createTransactionDto.Amount;

            account.Balance += balanceChange;
            _unitOfWork.Accounts.Update(account);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return transaction.ToDto();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TransactionDto?> UpdateTransactionAsync(UpdateTransactionDto updateTransactionDto, string userId)
    {
        if (updateTransactionDto.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive.");

        var existingTransaction = await _unitOfWork.Transactions.FirstOrDefaultAsync(
            t => t.Id == updateTransactionDto.Id && t.UserId == userId);

        if (existingTransaction == null)
            return null;

        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == updateTransactionDto.AccountId && a.UserId == userId);

        if (account == null)
            throw new InvalidOperationException("Account not found or access denied.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var oldBalanceChange = existingTransaction.Type == TransactionType.Income 
                ? -existingTransaction.Amount 
                : existingTransaction.Amount;

            var newBalanceChange = updateTransactionDto.Type == TransactionType.Income 
                ? updateTransactionDto.Amount 
                : -updateTransactionDto.Amount;

            var totalBalanceChange = oldBalanceChange + newBalanceChange;

            existingTransaction.Amount = updateTransactionDto.Amount;
            existingTransaction.Description = updateTransactionDto.Description;
            existingTransaction.TransactionDate = updateTransactionDto.TransactionDate;
            existingTransaction.Type = updateTransactionDto.Type;
            existingTransaction.IsRecurring = updateTransactionDto.IsRecurring;
            existingTransaction.RecurrenceType = updateTransactionDto.RecurrenceType;
            existingTransaction.AccountId = updateTransactionDto.AccountId;
            existingTransaction.CategoryId = updateTransactionDto.CategoryId;
            existingTransaction.InstallmentId = updateTransactionDto.InstallmentId;

            _unitOfWork.Transactions.Update(existingTransaction);

            account.Balance += totalBalanceChange;
            _unitOfWork.Accounts.Update(account);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return existingTransaction.ToDto();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        var transaction = await _unitOfWork.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
            return false;

        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(a => a.Id == transaction.AccountId);

        if (account == null)
            throw new InvalidOperationException("Associated account not found.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var balanceChange = transaction.Type == TransactionType.Income 
                ? -transaction.Amount 
                : transaction.Amount;

            account.Balance += balanceChange;
            _unitOfWork.Accounts.Update(account);

            _unitOfWork.Transactions.Remove(transaction);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ProcessRecurringTransactionsAsync()
    {
        var recurringTransactions = await _unitOfWork.Transactions.GetRecurringTransactionsAsync();

        foreach (var transaction in recurringTransactions)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var newTransaction = new Transaction
                {
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    TransactionDate = DateTime.UtcNow,
                    Type = transaction.Type,
                    IsRecurring = false,
                    UserId = transaction.UserId,
                    AccountId = transaction.AccountId,
                    CategoryId = transaction.CategoryId,
                    InstallmentId = transaction.InstallmentId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Transactions.AddAsync(newTransaction);

                transaction.NextRecurrenceDate = transaction.RecurrenceType switch
                {
                    RecurrenceType.Monthly => transaction.NextRecurrenceDate?.AddMonths(1),
                    RecurrenceType.Weekly => transaction.NextRecurrenceDate?.AddDays(7),
                    RecurrenceType.Yearly => transaction.NextRecurrenceDate?.AddYears(1),
                    _ => null
                };

                _unitOfWork.Transactions.Update(transaction);

                var account = await _unitOfWork.Accounts.GetByIdAsync(transaction.AccountId);
                if (account != null)
                {
                    var balanceChange = transaction.Type == TransactionType.Income 
                        ? transaction.Amount 
                        : -transaction.Amount;

                    account.Balance += balanceChange;
                    _unitOfWork.Accounts.Update(account);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }
    }
}