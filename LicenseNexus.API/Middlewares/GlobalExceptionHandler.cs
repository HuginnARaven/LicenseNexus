using FluentValidation;
using LicenseNexus.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        int statusCode;
        string detail;
        IDictionary<string, object?>? extensions = null;
        
        switch (exception)
        {
            // FluentValidation
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                detail = "One or more validation errors occurred.";
                
                var errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                extensions = new Dictionary<string, object?> { { "errors", errors } };
                break;
            
            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                detail = exception.Message;
                break;

            case ConflictException:
                statusCode = StatusCodes.Status409Conflict;
                detail = exception.Message;
                break;

            case BadRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                detail = exception.Message;
                break;
            
            case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                statusCode = StatusCodes.Status409Conflict;
                detail = "A record with this unique identifier or name already exists in the database.";
                break;

            case DbUpdateException dbEx:
                statusCode = StatusCodes.Status409Conflict;
                detail = "A database constraint violation occurred. This usually means a record with the same unique key already exists.";
                break;
            
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                detail = "An unexpected error occurred processing your request.";
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        
        if (extensions != null)
        {
            foreach (var ext in extensions)
            {
                problemDetails.Extensions.Add(ext);
            }
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
