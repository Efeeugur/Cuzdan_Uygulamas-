using Microsoft.EntityFrameworkCore.Storage;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.DataAccess.Interfaces;
using Cüzdan_Uygulaması.DataAccess.Repositories;
using Cüzdan_Uygulaması.Models;

namespace Cüzdan_Uygulaması.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<Repository<Account>> _accountLogger;
    private readonly ILogger<Repository<Transaction>> _transactionLogger;
    private readonly ILogger<Repository<Installment>> _installmentLogger;
    private IDbContextTransaction? _transaction;

    private IAccountRepository? _accounts;
    private ITransactionRepository? _transactions;
    private IInstallmentRepository? _installments;

    public UnitOfWork(ApplicationDbContext context, 
        ILogger<Repository<Account>> accountLogger,
        ILogger<Repository<Transaction>> transactionLogger,
        ILogger<Repository<Installment>> installmentLogger)
    {
        _context = context;
        _accountLogger = accountLogger;
        _transactionLogger = transactionLogger;
        _installmentLogger = installmentLogger;
    }

    public IAccountRepository Accounts =>
        _accounts ??= new AccountRepository(_context, _accountLogger);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_context, _transactionLogger);

    public IInstallmentRepository Installments =>
        _installments ??= new InstallmentRepository(_context, _installmentLogger);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}