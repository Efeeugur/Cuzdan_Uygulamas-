using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.DataAccess.Interfaces;
using Cüzdan_Uygulaması.Logging;
using System.Diagnostics;

namespace Cüzdan_Uygulaması.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;
    protected readonly string _tableName;

    public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
        _tableName = typeof(T).Name;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _dbSet.ToListAsync();
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > LoggerConstants.SlowQueryThresholdMs)
            {
                _logger.LogSlowQuery("GetAll", _tableName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("Query completed: {QueryType} on {TableName} returned {RecordCount} records in {ElapsedMs}ms",
                    "GetAll", _tableName, result.Count(), stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query failed: {QueryType} on {TableName} after {ElapsedMs}ms",
                "GetAll", _tableName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _dbSet.Where(predicate).ToListAsync();
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > LoggerConstants.SlowQueryThresholdMs)
            {
                _logger.LogSlowQuery("Find", _tableName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("Query completed: {QueryType} on {TableName} returned {RecordCount} records in {ElapsedMs}ms",
                    "Find", _tableName, result.Count(), stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query failed: {QueryType} on {TableName} after {ElapsedMs}ms",
                "Find", _tableName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _dbSet.FirstOrDefaultAsync(predicate);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > LoggerConstants.SlowQueryThresholdMs)
            {
                _logger.LogSlowQuery("FirstOrDefault", _tableName, stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query failed: {QueryType} on {TableName} after {ElapsedMs}ms",
                "FirstOrDefault", _tableName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _dbSet.AnyAsync(predicate);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > LoggerConstants.SlowQueryThresholdMs)
            {
                _logger.LogSlowQuery("Any", _tableName, stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query failed: {QueryType} on {TableName} after {ElapsedMs}ms",
                "Any", _tableName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _dbSet.CountAsync(predicate);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > LoggerConstants.SlowQueryThresholdMs)
            {
                _logger.LogSlowQuery("Count", _tableName, stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query failed: {QueryType} on {TableName} after {ElapsedMs}ms",
                "Count", _tableName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public virtual async Task AddAsync(T entity)
    {
        _logger.LogDebug("Adding entity to {TableName}", _tableName);
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        var count = entities.Count();
        _logger.LogDebug("Adding {Count} entities to {TableName}", count, _tableName);
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(T entity)
    {
        _logger.LogDebug("Updating entity in {TableName}", _tableName);
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}