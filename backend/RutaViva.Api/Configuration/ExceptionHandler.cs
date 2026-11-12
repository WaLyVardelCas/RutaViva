using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Api;

public static class ExceptionHandler
{
    public static void UseRutaVivaExceptions(this WebApplication app)
    {
        app.UseExceptionHandler(errors => errors.Run(async context =>
        {
            Exception? error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            (int statusCode, string? title) = error switch
            {
                BadHttpRequestException or JsonException => (StatusCodes.Status400BadRequest, "Datos inválidos"),
                NotFoundException => (StatusCodes.Status404NotFound, "No encontrado"),
                BusinessRuleException or DuplicateOpenOrderException or ConcurrencyConflictException
                    => (StatusCodes.Status409Conflict, "Regla de negocio"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno")
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new ApiError(
                title ?? "Error interno",
                statusCode == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error inesperado."
                    : error?.Message));
        }));
    }
}
