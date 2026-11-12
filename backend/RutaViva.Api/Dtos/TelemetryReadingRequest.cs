using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Lectura simulada de telemetría. Operación de demostración.</summary>
public sealed record TelemetryReadingRequest(
    [property: Description("Identificador del evento. Obligatorio. Una repetición devuelve el mismo resultado y no vuelve a aplicar la lectura.")] string? EventId,
    [property: Description("Placa del vehículo ya registrado. Obligatoria.")] string? Plate,
    [property: Description("Kilometraje reportado. Obligatorio. Solo se aplica si supera el actual.")] int? Mileage,
    [property: Description("Fecha de la lectura, YYYY-MM-DD. Obligatoria. No evalúa el vencimiento por tiempo.")] DateOnly? ReadingDate);
