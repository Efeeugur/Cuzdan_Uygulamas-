using Cüzdan_Uygulaması.BusinessLogic.DTOs;

namespace Cüzdan_Uygulaması.BusinessLogic.Interfaces;

public interface IInstallmentService
{
    Task<IEnumerable<InstallmentDto>> GetInstallmentsByUserIdAsync(string userId);
    Task<IEnumerable<InstallmentDto>> GetActiveInstallmentsAsync(string userId);
    Task<InstallmentDto?> GetInstallmentByIdAsync(int id, string userId);
    Task<InstallmentDto?> GetInstallmentWithTransactionsAsync(int id, string userId);
    Task<InstallmentDto> CreateInstallmentAsync(CreateInstallmentDto createInstallmentDto, string userId);
    Task<InstallmentDto?> UpdateInstallmentAsync(UpdateInstallmentDto updateInstallmentDto, string userId);
    Task<bool> DeleteInstallmentAsync(int id, string userId);
    Task<bool> ProcessInstallmentPaymentAsync(int installmentId, int accountId, string userId);
    Task<IEnumerable<InstallmentDto>> GetInstallmentsDueAsync(DateTime dueDate);
}