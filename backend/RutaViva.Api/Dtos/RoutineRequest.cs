using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Alta de una rutina de mantenimiento.</summary>
public sealed record RoutineRequest(
    [property: Description("Nombre. Obligatorio y único dentro del vehículo.")] string? Name,
    [property: Description("Intervalo en kilómetros. Opcional si se indica meses. Si se envía, debe ser mayor que cero.")] int? Kilometers,
    [property: Description("Intervalo en meses calendario. Opcional si se indica kilómetros. Si se envía, debe ser mayor que cero.")] int? Months,
    [property: Description("Fecha de inicio del primer ciclo. Si se omite, se usa la fecha de hoy en Bogotá.")] DateOnly? Date);
