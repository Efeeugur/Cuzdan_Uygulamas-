namespace Cüzdan_Uygulaması.Logging;

public static class LoggerConstants
{
    // Log Event Names
    public const string UserLogin = "UserLogin";
    public const string UserLogout = "UserLogout";
    public const string UserRegistration = "UserRegistration";
    public const string LoginFailed = "LoginFailed";
    
    public const string TransactionCreated = "TransactionCreated";
    public const string TransactionUpdated = "TransactionUpdated";
    public const string TransactionDeleted = "TransactionDeleted";
    public const string InsufficientFunds = "InsufficientFunds";
    
    public const string AccountCreated = "AccountCreated";
    public const string AccountUpdated = "AccountUpdated";
    public const string AccountDeleted = "AccountDeleted";
    public const string BalanceCalculated = "BalanceCalculated";
    
    public const string InstallmentCreated = "InstallmentCreated";
    public const string InstallmentPaid = "InstallmentPaid";
    public const string InstallmentUpdated = "InstallmentUpdated";
    public const string RecurringTransactionProcessed = "RecurringTransactionProcessed";
    
    public const string ReportGenerated = "ReportGenerated";
    public const string PdfGenerated = "PdfGenerated";
    
    public const string DatabaseQuery = "DatabaseQuery";
    public const string DatabaseQuerySlow = "DatabaseQuerySlow";
    
    public const string RequestStarted = "RequestStarted";
    public const string RequestCompleted = "RequestCompleted";
    public const string RequestFailed = "RequestFailed";
    
    public const string UnauthorizedAccess = "UnauthorizedAccess";
    public const string ValidationError = "ValidationError";
    public const string BusinessRuleViolation = "BusinessRuleViolation";
    
    // Property Names
    public const string UserId = "UserId";
    public const string Username = "Username";
    public const string TransactionId = "TransactionId";
    public const string AccountId = "AccountId";
    public const string InstallmentId = "InstallmentId";
    public const string Amount = "Amount";
    public const string Category = "Category";
    public const string Description = "Description";
    public const string RequestPath = "RequestPath";
    public const string RequestMethod = "RequestMethod";
    public const string StatusCode = "StatusCode";
    public const string ElapsedMs = "ElapsedMs";
    public const string IpAddress = "IpAddress";
    public const string UserAgent = "UserAgent";
    public const string TraceId = "TraceId";
    public const string QueryType = "QueryType";
    public const string TableName = "TableName";
    
    // Thresholds
    public const int SlowQueryThresholdMs = 1000;
    public const int SlowRequestThresholdMs = 5000;
}