using System.Net;
using System.Text.Json;
using Cüzdan_Uygulaması.Exceptions;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.Logging;

namespace Cüzdan_Uygulaması.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [LoggerConstants.TraceId] = context.TraceIdentifier,
                [LoggerConstants.UserId] = userId,
                [LoggerConstants.RequestPath] = context.Request.Path.Value ?? "",
                [LoggerConstants.RequestMethod] = context.Request.Method,
                [LoggerConstants.IpAddress] = ipAddress,
                [LoggerConstants.UserAgent] = userAgent,
                ["ExceptionType"] = exception.GetType().Name
            }))
            {
                // Log different exception types with appropriate levels
                switch (exception)
                {
                    case BaseCustomException customEx:
                        _logger.LogWarning(exception, 
                            "Business exception occurred: {ExceptionType} - {Message} (User: {UserId}, Path: {RequestPath})",
                            exception.GetType().Name, customEx.Message, userId, context.Request.Path);
                        break;
                        
                    case UnauthorizedAccessException:
                        _logger.LogWarning(exception,
                            "Unauthorized access attempt: User {UserId} from {IpAddress} tried to access {RequestPath}",
                            userId, ipAddress, context.Request.Path);
                        break;
                        
                    case ArgumentException argEx:
                        _logger.LogWarning(exception,
                            "Invalid argument provided: {Message} (User: {UserId}, Path: {RequestPath})",
                            argEx.Message, userId, context.Request.Path);
                        break;
                        
                    default:
                        _logger.LogError(exception,
                            "Unhandled system exception: {ExceptionType} - {Message} (User: {UserId}, Path: {RequestPath}, IP: {IpAddress})",
                            exception.GetType().Name, exception.Message, userId, context.Request.Path, ipAddress);
                        break;
                }
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = CreateErrorResponse(exception, context.TraceIdentifier);
        response.StatusCode = errorResponse.StatusCode;

        // Check if the request expects JSON response (API endpoints)
        var acceptHeader = context.Request.Headers.Accept.ToString();
        var isApiRequest = acceptHeader.Contains("application/json") || 
                          context.Request.Path.StartsWithSegments("/api") ||
                          context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (isApiRequest)
        {
            // Return JSON response for API requests
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await response.WriteAsync(jsonResponse);
        }
        else
        {
            // For MVC requests, redirect to error page with TempData
            context.Response.ContentType = "text/html";
            
            // Store error information in TempData for display
            if (context.Features.Get<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>() != null)
            {
                var tempDataFactory = context.RequestServices.GetService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
                var tempData = tempDataFactory?.GetTempData(context);
                
                if (tempData != null)
                {
                    tempData["ErrorMessage"] = errorResponse.Message;
                    tempData["ErrorCode"] = errorResponse.ErrorCode;
                    tempData.Keep();
                }
            }

            // Redirect to error page
            response.StatusCode = 200; // Avoid browser error pages
            response.Redirect($"/Home/Error?statusCode={errorResponse.StatusCode}&traceId={context.TraceIdentifier}");
        }
    }

    private ErrorResponse CreateErrorResponse(Exception exception, string traceId)
    {
        return exception switch
        {
            BaseCustomException customException => new ErrorResponse
            {
                Message = customException.Message,
                ErrorCode = customException.ErrorCode,
                StatusCode = customException.StatusCode,
                TraceId = traceId,
                ValidationErrors = customException is ValidationException validationEx ? validationEx.ValidationErrors : null,
                Details = _environment.IsDevelopment() ? GetExceptionDetails(customException) : null
            },
            
            ArgumentNullException => new ErrorResponse
            {
                Message = "Required parameter is missing.",
                ErrorCode = "MISSING_PARAMETER",
                StatusCode = (int)HttpStatusCode.BadRequest,
                TraceId = traceId
            },
            
            ArgumentException argEx => new ErrorResponse
            {
                Message = "Invalid argument provided.",
                ErrorCode = "INVALID_ARGUMENT",
                StatusCode = (int)HttpStatusCode.BadRequest,
                TraceId = traceId,
                Details = _environment.IsDevelopment() ? GetExceptionDetails(argEx) : null
            },
            
            InvalidOperationException invOpEx => new ErrorResponse
            {
                Message = invOpEx.Message,
                ErrorCode = "INVALID_OPERATION",
                StatusCode = (int)HttpStatusCode.BadRequest,
                TraceId = traceId,
                Details = _environment.IsDevelopment() ? GetExceptionDetails(invOpEx) : null
            },
            
            UnauthorizedAccessException => new ErrorResponse
            {
                Message = "You are not authorized to perform this operation.",
                ErrorCode = "UNAUTHORIZED",
                StatusCode = (int)HttpStatusCode.Unauthorized,
                TraceId = traceId
            },
            
            _ => new ErrorResponse
            {
                Message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
                ErrorCode = "INTERNAL_SERVER_ERROR",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                TraceId = traceId,
                Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
            }
        };
    }

    private object GetExceptionDetails(Exception exception)
    {
        return new
        {
            type = exception.GetType().Name,
            stackTrace = exception.StackTrace,
            source = exception.Source,
            innerException = exception.InnerException?.Message
        };
    }
}