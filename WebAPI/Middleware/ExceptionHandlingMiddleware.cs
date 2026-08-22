using Application.Exceptions;
using Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace WebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;
            IReadOnlyDictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case DomainException domainEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = domainEx.Message;
                    _logger.LogWarning(domainEx, "Domain exception: {Message}", domainEx.Message);
                    break;

                case ApplicationLayerException appEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = appEx.Message;
                    _logger.LogWarning(appEx, "Application exception: {Message}", appEx.Message);
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    _logger.LogWarning(argEx, "Argument exception: {Message}", argEx.Message);
                    break;

                case KeyNotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundEx.Message;
                    _logger.LogWarning(notFoundEx, "Not found: {Message}", notFoundEx.Message);
                    break;

                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validation Error";
                    errors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        ) as IReadOnlyDictionary<string, string[]>;
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsEnvironment("Testing") == true)
                    {
                        message = exception.ToString(); 
                    }
                    else
                    {
                        message = "Произошла внутренняя ошибка сервера";
                    }
                    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                    break;
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                title = message,
                errors = (object?)null
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }
    }
}
