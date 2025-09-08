using System.Diagnostics;
using Cüzdan_Uygulaması.Logging;

namespace Cüzdan_Uygulaması.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path.Value ?? "";
        var requestMethod = context.Request.Method;
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userId = context.User?.Identity?.Name ?? "Anonymous";

        // Skip logging for static assets and health checks
        if (ShouldSkipLogging(requestPath))
        {
            await _next(context);
            return;
        }

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [LoggerConstants.RequestPath] = requestPath,
            [LoggerConstants.RequestMethod] = requestMethod,
            [LoggerConstants.IpAddress] = ipAddress,
            [LoggerConstants.UserAgent] = userAgent,
            [LoggerConstants.UserId] = userId,
            [LoggerConstants.TraceId] = context.TraceIdentifier
        }))
        {
            _logger.LogInformation(LoggerConstants.RequestStarted,
                "Request started: {Method} {RequestPath} from {IpAddress} (User: {UserId})",
                requestMethod, requestPath, ipAddress, userId);

            var originalResponseBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Log slow requests
                if (elapsedMs > LoggerConstants.SlowRequestThresholdMs)
                {
                    _logger.LogWarning("Slow request detected: {Method} {RequestPath} took {ElapsedMs}ms (Status: {StatusCode})",
                        requestMethod, requestPath, elapsedMs, statusCode);
                }

                _logger.LogRequestCompleted(requestPath, requestMethod, statusCode, elapsedMs);

                // Log specific metrics for financial operations
                if (IsFinancialOperation(requestPath))
                {
                    _logger.LogInformation("Financial operation completed: {Method} {RequestPath} by user {UserId} - Status: {StatusCode}, Duration: {ElapsedMs}ms",
                        requestMethod, requestPath, userId, statusCode, elapsedMs);
                }
            }
        }
    }

    private static bool ShouldSkipLogging(string requestPath)
    {
        var pathsToSkip = new[]
        {
            "/favicon.ico",
            "/robots.txt",
            "/_framework/",
            "/css/",
            "/js/",
            "/images/",
            "/lib/",
            "/fonts/",
            ".css",
            ".js",
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".ico",
            ".svg",
            ".woff",
            ".woff2",
            ".ttf",
            ".eot"
        };

        return pathsToSkip.Any(skip => requestPath.Contains(skip, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFinancialOperation(string requestPath)
    {
        var financialPaths = new[]
        {
            "/wallet/",
            "/transaction/",
            "/installment/",
            "/reports/"
        };

        return financialPaths.Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }
}