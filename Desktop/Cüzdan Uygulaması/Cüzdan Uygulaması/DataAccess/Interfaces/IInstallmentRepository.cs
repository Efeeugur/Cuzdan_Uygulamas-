using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.DataAccess.Interfaces;

public interface IInstallmentRepository : IRepository<Installment>
{
    Task<IEnumerable<Installment>> GetInstallmentsByUserIdAsync(string userId);
    Task<IEnumerable<Installment>> GetActiveInstallmentsAsync(string userId);
    Task<IEnumerable<Installment>> GetInstallmentsDueAsync(DateTime dueDate);
    Task<Installment?> GetInstallmentWithTransactionsAsync(int installmentId);
}