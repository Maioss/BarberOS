using System.Net;
using System.Text.Json;
using BarberOS.Api.Common;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = Map(exception);

        if (statusCode >= 500)
            _logger.LogError(exception, "Error no controlado");
        else
            _logger.LogWarning("Error de negocio: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = ApiResponse<object>.Fail(message);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
    }

    private static (int StatusCode, string Message) Map(Exception ex) => ex switch
    {
        NotFoundException     => ((int)HttpStatusCode.NotFound,            ex.Message),
        ConflictException     => ((int)HttpStatusCode.Conflict,            ex.Message),
        BusinessRuleException => ((int)HttpStatusCode.BadRequest,          ex.Message),
        DomainException       => ((int)HttpStatusCode.BadRequest,          ex.Message),
        _                     => ((int)HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.")
    };
}
