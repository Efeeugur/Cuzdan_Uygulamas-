using System.Diagnostics;

namespace Cüzdan_Uygulaması.Logging;

public static class LoggerExtensions
{
    // Authentication & Authorization Extensions
    public static void LogUserLogin(this ILogger logger, string userId, string username, string ipAddress)
    {
        logger.LogInformation(LoggerConstants.UserLogin, 
            "User {Username} (ID: {UserId}) logged in from {IpAddress}",
            username, userId, ipAddress);
    }
    
    public static void LogUserLogout(this ILogger logger, string userId, string username)
    {
        logger.LogInformation(LoggerConstants.UserLogout,
            "User {Username} (ID: {UserId}) logged out",
            username, userId);
    }
    
    public static void LogUserRegistration(this ILogger logger, string userId, string username, string ipAddress)
    {
        logger.LogInformation(LoggerConstants.UserRegistration,
            "New user {Username} (ID: {UserId}) registered from {IpAddress}",
            username, userId, ipAddress);
    }
    
    public static void LogLoginFailed(this ILogger logger, string username, string ipAddress, string reason)
    {
        logger.LogWarning(LoggerConstants.LoginFailed,
            "Login failed for {Username} from {IpAddress}. Reason: {Reason}",
            username, ipAddress, reason);
    }
    
    public static void LogUnauthorizedAccess(this ILogger logger, string userId, string requestPath, string ipAddress)
    {
        logger.LogWarning(LoggerConstants.UnauthorizedAccess,
            "Unauthorized access attempt by user {UserId} to {RequestPath} from {IpAddress}",
            userId, requestPath, ipAddress);
    }
    
    // Transaction Extensions
    public static void LogTransactionCreated(this ILogger logger, string userId, int transactionId, decimal amount, string category, string description)
    {
        logger.LogInformation(LoggerConstants.TransactionCreated,
            "Transaction created by user {UserId}: ID={TransactionId}, Amount={Amount:C}, Category={Category}, Description={Description}",
            userId, transactionId, amount, category, description);
    }
    
    public static void LogTransactionUpdated(this ILogger logger, string userId, int transactionId, decimal oldAmount, decimal newAmount)
    {
        logger.LogInformation(LoggerConstants.TransactionUpdated,
            "Transaction {TransactionId} updated by user {UserId}: Amount changed from {OldAmount:C} to {NewAmount:C}",
            transactionId, userId, oldAmount, newAmount);
    }
    
    public static void LogTransactionDeleted(this ILogger logger, string userId, int transactionId, decimal amount)
    {
        logger.LogInformation(LoggerConstants.TransactionDeleted,
            "Transaction {TransactionId} deleted by user {UserId}: Amount={Amount:C}",
            transactionId, userId, amount);
    }
    
    public static void LogInsufficientFunds(this ILogger logger, string userId, int accountId, decimal attemptedAmount, decimal availableBalance)
    {
        logger.LogWarning(LoggerConstants.InsufficientFunds,
            "Insufficient funds: User {UserId} attempted to spend {AttemptedAmount:C} from account {AccountId} with balance {AvailableBalance:C}",
            userId, attemptedAmount, accountId, availableBalance);
    }
    
    // Account Extensions
    public static void LogAccountCreated(this ILogger logger, string userId, int accountId, string accountName, decimal initialBalance)
    {
        logger.LogInformation(LoggerConstants.AccountCreated,
            "Account created by user {UserId}: ID={AccountId}, Name={AccountName}, InitialBalance={InitialBalance:C}",
            userId, accountId, accountName, initialBalance);
    }
    
    public static void LogBalanceCalculated(this ILogger logger, string userId, int accountId, decimal balance)
    {
        logger.LogDebug(LoggerConstants.BalanceCalculated,
            "Balance calculated for account {AccountId} (user {UserId}): {Balance:C}",
            accountId, userId, balance);
    }
    
    // Performance Extensions
    public static void LogSlowQuery(this ILogger logger, string queryType, string tableName, long elapsedMs)
    {
        logger.LogWarning(LoggerConstants.DatabaseQuerySlow,
            "Slow database query detected: {QueryType} on {TableName} took {ElapsedMs}ms",
            queryType, tableName, elapsedMs);
    }
    
    public static void LogRequestCompleted(this ILogger logger, string requestPath, string method, int statusCode, long elapsedMs)
    {
        var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        var eventName = statusCode >= 400 ? LoggerConstants.RequestFailed : LoggerConstants.RequestCompleted;
        
        logger.Log(logLevel, eventName,
            "Request completed: {Method} {RequestPath} responded {StatusCode} in {ElapsedMs}ms",
            method, requestPath, statusCode, elapsedMs);
    }
    
    // Business Logic Extensions
    public static void LogBusinessRuleViolation(this ILogger logger, string userId, string ruleName, string details)
    {
        logger.LogWarning(LoggerConstants.BusinessRuleViolation,
            "Business rule violation for user {UserId}: {RuleName} - {Details}",
            userId, ruleName, details);
    }
    
    public static void LogValidationError(this ILogger logger, string userId, string field, string error)
    {
        logger.LogWarning(LoggerConstants.ValidationError,
            "Validation error for user {UserId}: Field={Field}, Error={Error}",
            userId, field, error);
    }
    
    // Report Extensions
    public static void LogReportGenerated(this ILogger logger, string userId, string reportType, int recordCount, long elapsedMs)
    {
        logger.LogInformation(LoggerConstants.ReportGenerated,
            "Report generated for user {UserId}: Type={ReportType}, Records={RecordCount}, ElapsedMs={ElapsedMs}",
            userId, reportType, recordCount, elapsedMs);
    }
    
    public static void LogPdfGenerated(this ILogger logger, string userId, string reportType, long fileSizeBytes, long elapsedMs)
    {
        logger.LogInformation(LoggerConstants.PdfGenerated,
            "PDF generated for user {UserId}: Type={ReportType}, FileSize={FileSizeBytes}b, ElapsedMs={ElapsedMs}",
            userId, reportType, fileSizeBytes, elapsedMs);
    }
    
    // Utility Extensions
    public static IDisposable? BeginScopeWithUserId(this ILogger logger, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return null;
            
        return logger.BeginScope(new Dictionary<string, object>
        {
            [LoggerConstants.UserId] = userId
        });
    }
    
    public static IDisposable? TimeOperation(this ILogger logger, string operationName)
    {
        return new TimedOperation(logger, operationName);
    }
}

public class TimedOperation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _disposed = false;
    
    public TimedOperation(ILogger logger, string operationName)
    {
        _logger = logger;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _stopwatch.Stop();
        _logger.LogDebug("Operation {OperationName} completed in {ElapsedMs}ms",
            _operationName, _stopwatch.ElapsedMilliseconds);
        
        _disposed = true;
    }
}