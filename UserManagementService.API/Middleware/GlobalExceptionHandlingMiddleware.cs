using System.Net;
using System.Text.Json;
using UserManagementService.Application.Common;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using AppValidationException =
    UserManagementService.Application.Common.Exceptions.ValidationException;

namespace UserManagementService.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // Fire and forget — never blocks the response
            await PublishErrorLogAsync(context, ex);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task PublishErrorLogAsync(HttpContext context, Exception ex)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var publisher = scope.ServiceProvider
                .GetRequiredService<ILogPublisher>();

            var userId = context.User.FindFirst("UserId")?.Value;
            Guid.TryParse(
                context.User.FindFirst("TenantId")?.Value,
                out var tenantId);
            Guid.TryParse(
                context.User.FindFirst("BranchId")?.Value,
                out var branchId);

            // Determine status code based on exception type
            var statusCode = ex switch
            {
                NotFoundException => 404,
                ConflictException => 409,
                AppValidationException or BadRequestException => 400,
                UnauthorizedException or ForbiddenException or UnauthorizedAccessException => 403,
                _ => 500
            };

            publisher.PublishError(new ErrorLogEvent
            {
                Timestamp = DateTime.UtcNow,
                Severity = statusCode >= 500 ? 3 : 2,   // Error for 5xx, Warning for others
                Source = 0,   // Backend
                Category = GetErrorCategory(ex),
                ServiceName = "UserManagementService",
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                StatusCode = statusCode,
                UserId = userId,
                TenantId = tenantId == Guid.Empty ? null : tenantId,
                BranchId = branchId == Guid.Empty ? null : branchId,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString()
            });
        }
        catch { /* silent — never affect response */ }
    }

    private static int GetErrorCategory(Exception ex)
    {
        return ex switch
        {
            ValidationException or BadRequestException => 1,  // ValidationError
            NotFoundException => 2,  // NotFoundError
            ConflictException => 3,  // ConflictError
            UnauthorizedException or ForbiddenException or UnauthorizedAccessException => 4,  // AuthorizationError
            _ => 0  // ServerError
        };
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception ex)
    {
        var (status, msg, errors) = ex switch
        {
            NotFoundException e =>
                (HttpStatusCode.NotFound, e.Message, new List<string>()),
            ConflictException e =>
                (HttpStatusCode.Conflict, e.Message, new List<string>()),
            AppValidationException e =>
                (HttpStatusCode.BadRequest, "Validation failed", e.Errors),
            BadRequestException e =>
                (HttpStatusCode.BadRequest, e.Message, e.Errors),
            ForbiddenException e =>
                (HttpStatusCode.Forbidden, e.Message, new List<string>()),
            UnauthorizedException e =>
                (HttpStatusCode.Unauthorized, e.Message, new List<string>()),
            UnauthorizedAccessException e =>
                (HttpStatusCode.Forbidden, e.Message, new List<string>()),
            _ =>
                (HttpStatusCode.InternalServerError,
                 "An unexpected error occurred.",
                 new List<string>())
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        var response = ApiResponse<object>.Fail(
            (int)status, msg, errors);
        await context.Response.WriteAsJsonAsync(response,
            new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
