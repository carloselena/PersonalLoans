using Blocks.Application.Exceptions;
using Blocks.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PersonalLoans.Api.Infrastructure.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            BusinessRuleViolationException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio violada"),
            InvalidDomainValueException => (StatusCodes.Status400BadRequest, "Valor inválido"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };
        
        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Ocurrió un error inesperado: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}