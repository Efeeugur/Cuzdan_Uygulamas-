using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(string userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByAccountIdAsync(int accountId, string userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByCategoryIdAsync(int categoryId, string userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<TransactionDto?> GetTransactionByIdAsync(int id, string userId);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(string userId);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto, string userId);
    Task<TransactionDto?> UpdateTransactionAsync(UpdateTransactionDto updateTransactionDto, string userId);
    Task<bool> DeleteTransactionAsync(int id, string userId);
    Task ProcessRecurringTransactionsAsync();
}