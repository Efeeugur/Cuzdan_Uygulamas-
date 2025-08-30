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
        // CategoryId now refers to SimpleCategoryService categories (1-35)
        // No database validation needed for predefined categories
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
        var transactions = await _unitOfWork.Transactions.GetTransactionsWithDetailsAsync(userId);
        var transaction = transactions.FirstOrDefault(t => t.Id == id);
        
        if (transaction == null)
            return null;
            
        var dto = transaction.ToDto();
        
        // Manually populate account and category names for display
        if (transaction.Account != null)
        {
            dto.Account = transaction.Account.ToBasicDto();
        }
        
        if (transaction.Category != null)
        {
            dto.Category = transaction.Category.ToBasicDto();
        }
        else if (dto.CategoryId.HasValue && dto.CategoryId.Value <= 35)
        {
            // Handle SimpleCategoryService categories (hardcoded categories 1-35)
            var simpleCategoryService = new SimpleCategoryService();
            dto.Category = new CategoryDto
            {
                Id = dto.CategoryId.Value,
                Name = simpleCategoryService.GetCategoryName(dto.CategoryId.Value),
                Type = dto.CategoryId.Value <= 5 ? Models.CategoryType.Income : Models.CategoryType.Expense,
                Transactions = new List<TransactionDto>()
            };
        }
        
        return dto;
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

    public async Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(string userId, TransactionFilterDto filter)
    {
        var transactions = await _unitOfWork.Transactions.GetTransactionsWithDetailsAsync(userId);

        var query = transactions.AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.IsRecurring.HasValue)
            query = query.Where(t => t.IsRecurring == filter.IsRecurring.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(t => t.Description.ToLower().Contains(searchTerm));
        }

        return query.OrderByDescending(t => t.TransactionDate)
                   .Select(t => t.ToDto())
                   .ToList();
    }

    public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(string userId, TransactionFilterDto filter)
    {
        var filteredTransactions = await GetFilteredTransactionsAsync(userId, filter);
        
        var totalIncome = filteredTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);
            
        var totalExpense = filteredTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            TransactionCount = filteredTransactions.Count()
        };
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto, string userId)
    {
        if (createTransactionDto.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive.");

        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == createTransactionDto.AccountId && a.UserId == userId);

        if (account == null)
            throw new InvalidOperationException($"Account with ID {createTransactionDto.AccountId} not found or access denied. Please ensure you have created an account first.");

        if (createTransactionDto.CategoryId.HasValue)
        {
            // Validate SimpleCategoryService categories (1-35)
            if (createTransactionDto.CategoryId.Value < 1 || createTransactionDto.CategoryId.Value > 35)
            {
                throw new InvalidOperationException("Invalid category ID. Must be between 1 and 35.");
            }
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
            
            // Note: We keep CategoryId even for SimpleCategoryService categories (1-25)
            // The foreign key constraint allows null, so EF won't enforce the relationship
            // We'll handle display logic in the service layer
            
            // Log transaction details for debugging
            Console.WriteLine($"Creating Transaction: UserId={transaction.UserId}, AccountId={transaction.AccountId}, Amount={transaction.Amount}, Type={transaction.Type}, CategoryId={transaction.CategoryId}");
            
            await _unitOfWork.Transactions.AddAsync(transaction);

            var balanceChange = createTransactionDto.Type == TransactionType.Income 
                ? createTransactionDto.Amount 
                : -createTransactionDto.Amount;

            account.Balance += balanceChange;
            _unitOfWork.Accounts.Update(account);

            Console.WriteLine($"Saving changes - Account Balance will be: {account.Balance}");
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