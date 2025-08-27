using Microsoft.EntityFrameworkCore;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.DataAccess.Interfaces;

namespace Cüzdan_Uygulaması.DataAccess.Repositories;

public class InstallmentRepository : Repository<Installment>, IInstallmentRepository
{
    public InstallmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Installment>> GetInstallmentsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(i => i.UserId == userId)
            .Include(i => i.Category)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Installment>> GetActiveInstallmentsAsync(string userId)
    {
        return await _dbSet
            .Where(i => i.UserId == userId && i.Status == InstallmentStatus.Active)
            .Include(i => i.Category)
            .OrderBy(i => i.NextPaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Installment>> GetInstallmentsDueAsync(DateTime dueDate)
    {
        return await _dbSet
            .Where(i => i.Status == InstallmentStatus.Active && i.NextPaymentDate <= dueDate)
            .Include(i => i.User)
            .Include(i => i.Category)
            .ToListAsync();
    }

    public async Task<Installment?> GetInstallmentWithTransactionsAsync(int installmentId)
    {
        return await _dbSet
            .Include(i => i.Transactions)
                .ThenInclude(t => t.Account)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == installmentId);
    }
}