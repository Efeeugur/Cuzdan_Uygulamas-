using Cüzdan_Uygulaması.Logging;

namespace Cüzdan_Uygulaması.Services;

public interface ISecurityAuditService
{
    void LogSecurityEvent(string eventType, string userId, string ipAddress, string details, string? additionalData = null);
    void LogDataAccess(string userId, string dataType, string operation, string resourceId, bool success);
    void LogPrivilegeEscalation(string userId, string attemptedAction, string ipAddress);
    void LogSuspiciousActivity(string eventType, string userId, string ipAddress, string details);
}

public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ILogger<SecurityAuditService> logger)
    {
        _logger = logger;
    }

    public void LogSecurityEvent(string eventType, string userId, string ipAddress, string details, string? additionalData = null)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = eventType,
            [LoggerConstants.UserId] = userId,
            [LoggerConstants.IpAddress] = ipAddress,
            ["SecurityEvent"] = true
        }))
        {
            _logger.LogWarning("Security Event: {EventType} - User: {UserId}, IP: {IpAddress}, Details: {Details}, Additional: {AdditionalData}",
                eventType, userId, ipAddress, details, additionalData ?? "None");
        }
    }

    public void LogDataAccess(string userId, string dataType, string operation, string resourceId, bool success)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [LoggerConstants.UserId] = userId,
            ["DataType"] = dataType,
            ["Operation"] = operation,
            ["ResourceId"] = resourceId,
            ["DataAccess"] = true
        }))
        {
            var logLevel = success ? LogLevel.Information : LogLevel.Warning;
            _logger.Log(logLevel, "Data Access: User {UserId} {Operation} {DataType} {ResourceId} - {Result}",
                userId, operation, dataType, resourceId, success ? "SUCCESS" : "FAILED");
        }
    }

    public void LogPrivilegeEscalation(string userId, string attemptedAction, string ipAddress)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [LoggerConstants.UserId] = userId,
            [LoggerConstants.IpAddress] = ipAddress,
            ["SecurityThreat"] = true,
            ["ThreatLevel"] = "HIGH"
        }))
        {
            _logger.LogCritical("PRIVILEGE ESCALATION ATTEMPT: User {UserId} attempted {AttemptedAction} from {IpAddress}",
                userId, attemptedAction, ipAddress);
        }
    }

    public void LogSuspiciousActivity(string eventType, string userId, string ipAddress, string details)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["EventType"] = eventType,
            [LoggerConstants.UserId] = userId,
            [LoggerConstants.IpAddress] = ipAddress,
            ["SuspiciousActivity"] = true
        }))
        {
            _logger.LogWarning("Suspicious Activity: {EventType} - User: {UserId}, IP: {IpAddress}, Details: {Details}",
                eventType, userId, ipAddress, details);
        }
    }
}