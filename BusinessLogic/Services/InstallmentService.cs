using Cüzdan_Uygulaması.BusinessLogic.DTOs;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Mappers;
using Cüzdan_Uygulaması.DataAccess.Interfaces;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.BusinessLogic.Services;

namespace Cüzdan_Uygulaması.BusinessLogic.Services;

public class InstallmentService : IInstallmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public InstallmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<InstallmentDto>> GetInstallmentsByUserIdAsync(string userId)
    {
        var installments = await _unitOfWork.Installments.GetInstallmentsByUserIdAsync(userId);
        return installments.Select(i => i.ToDto());
    }

    public async Task<IEnumerable<InstallmentDto>> GetActiveInstallmentsAsync(string userId)
    {
        var installments = await _unitOfWork.Installments.GetActiveInstallmentsAsync(userId);
        return installments.Select(i => i.ToDto());
    }

    public async Task<InstallmentDto?> GetInstallmentByIdAsync(int id, string userId)
    {
        var installment = await _unitOfWork.Installments.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        return installment?.ToDto();
    }

    public async Task<InstallmentDto?> GetInstallmentWithTransactionsAsync(int id, string userId)
    {
        var installment = await _unitOfWork.Installments.GetInstallmentWithTransactionsAsync(id);

        if (installment == null || installment.UserId != userId)
            return null;

        return installment.ToDto();
    }

    public async Task<InstallmentDto> CreateInstallmentAsync(CreateInstallmentDto createInstallmentDto, string userId)
    {
        if (createInstallmentDto.TotalAmount <= 0)
            throw new ArgumentException("Total amount must be positive.");

        if (createInstallmentDto.TotalInstallments <= 0)
            throw new ArgumentException("Total installments must be positive.");

        if (createInstallmentDto.InterestRate < 0)
            throw new ArgumentException("Interest rate cannot be negative.");

        if (createInstallmentDto.CategoryId.HasValue)
        {
            // Validate SimpleCategoryService categories (1-35)
            if (createInstallmentDto.CategoryId.Value < 1 || createInstallmentDto.CategoryId.Value > 35)
            {
                throw new InvalidOperationException("Invalid category ID. Must be between 1 and 35.");
            }
        }

        var installment = createInstallmentDto.ToEntity(userId);
        await _unitOfWork.Installments.AddAsync(installment);
        await _unitOfWork.SaveChangesAsync();

        return installment.ToDto();
    }

    public async Task<InstallmentDto?> UpdateInstallmentAsync(UpdateInstallmentDto updateInstallmentDto, string userId)
    {
        var installment = await _unitOfWork.Installments.FirstOrDefaultAsync(
            i => i.Id == updateInstallmentDto.Id && i.UserId == userId);

        if (installment == null)
            return null;

        if (installment.Status == InstallmentStatus.Completed)
            throw new InvalidOperationException("Cannot update completed installment.");

        if (updateInstallmentDto.InterestRate < 0)
            throw new ArgumentException("Interest rate cannot be negative.");

        if (updateInstallmentDto.CategoryId.HasValue)
        {
            // Validate SimpleCategoryService categories (1-35)
            if (updateInstallmentDto.CategoryId.Value < 1 || updateInstallmentDto.CategoryId.Value > 35)
            {
                throw new InvalidOperationException("Invalid category ID. Must be between 1 and 35.");
            }
        }

        installment.Description = updateInstallmentDto.Description;
        installment.InterestRate = updateInstallmentDto.InterestRate;
        installment.NextPaymentDate = updateInstallmentDto.NextPaymentDate;
        installment.CategoryId = updateInstallmentDto.CategoryId;

        _unitOfWork.Installments.Update(installment);
        await _unitOfWork.SaveChangesAsync();

        return installment.ToDto();
    }

    public async Task<bool> DeleteInstallmentAsync(int id, string userId)
    {
        var installment = await _unitOfWork.Installments.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (installment == null)
            return false;

        var hasTransactions = await _unitOfWork.Transactions.AnyAsync(t => t.InstallmentId == id);
        if (hasTransactions)
            throw new InvalidOperationException("Cannot delete installment with existing transactions.");

        _unitOfWork.Installments.Remove(installment);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ProcessInstallmentPaymentAsync(int installmentId, int accountId, string userId)
    {
        return await ProcessInstallmentPaymentAsync(installmentId, accountId, userId, DateTime.Now, null);
    }

    public async Task<bool> ProcessInstallmentPaymentAsync(int installmentId, int accountId, string userId, DateTime paymentDate, string? notes)
    {
        var installment = await _unitOfWork.Installments.FirstOrDefaultAsync(
            i => i.Id == installmentId && i.UserId == userId);

        if (installment == null || installment.Status == InstallmentStatus.Completed)
            return false;

        var account = await _unitOfWork.Accounts.FirstOrDefaultAsync(
            a => a.Id == accountId && a.UserId == userId);

        if (account == null)
            return false;

        if (account.Balance < installment.MonthlyPayment)
            throw new InvalidOperationException("Bu ödeme için hesabınızda yeterli bakiye bulunmuyor.");

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var transactionDescription = $"Taksit ödemesi: {installment.Description}";
            if (!string.IsNullOrWhiteSpace(notes))
            {
                transactionDescription += $" - {notes}";
            }

            var transaction = new Transaction
            {
                Amount = installment.MonthlyPayment,
                Description = transactionDescription,
                TransactionDate = paymentDate,
                Type = TransactionType.Expense,
                UserId = userId,
                AccountId = accountId,
                CategoryId = installment.CategoryId,
                InstallmentId = installmentId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Transactions.AddAsync(transaction);

            account.Balance -= installment.MonthlyPayment;
            _unitOfWork.Accounts.Update(account);

            installment.RemainingInstallments--;
            installment.NextPaymentDate = installment.NextPaymentDate.AddMonths(1);

            if (installment.RemainingInstallments <= 0)
            {
                installment.Status = InstallmentStatus.Completed;
            }

            _unitOfWork.Installments.Update(installment);

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

    public async Task<IEnumerable<InstallmentDto>> GetInstallmentsDueAsync(DateTime dueDate)
    {
        var installments = await _unitOfWork.Installments.GetInstallmentsDueAsync(dueDate);
        return installments.Select(i => i.ToDto());
    }
}