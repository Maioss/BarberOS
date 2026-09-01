using System.Net;
using System.Text.Json;
using BarberOS.Api.Common;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Api.Middleware
{

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
            var (statusCode, message, errors) = Map(exception);

            if (statusCode >= 500)
                _logger.LogError(exception, "Error no controlado");
            else
                _logger.LogWarning("Error de negocio: {Message}", exception.Message);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("La respuesta ya habia empezado; no se pudo devolver el error al cliente.");
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = ApiResponse<object>.Fail(message, errors);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
        }

        private static (int StatusCode, string Message, List<string>? Errors) Map(Exception ex) => ex switch
        {
            FluentValidation.ValidationException ve =>
                ((int)HttpStatusCode.BadRequest, "Datos inválidos.", ve.Errors.Select(e => e.ErrorMessage).Distinct().ToList()),
            NotFoundException     => ((int)HttpStatusCode.NotFound,            ex.Message, null),
            ConflictException     => ((int)HttpStatusCode.Conflict,            ex.Message, null),
            UnauthorizedException => ((int)HttpStatusCode.Unauthorized,        ex.Message, null),
            ForbiddenException    => ((int)HttpStatusCode.Forbidden,           ex.Message, null),
            BusinessRuleException => ((int)HttpStatusCode.BadRequest,          ex.Message, null),
            DomainException       => ((int)HttpStatusCode.BadRequest,          ex.Message, null),
            _                     => ((int)HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.", null)
        };
    }
}
