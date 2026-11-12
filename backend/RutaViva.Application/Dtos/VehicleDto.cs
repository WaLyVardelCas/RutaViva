using System.ComponentModel;

namespace RutaViva.Application;

public sealed record VehicleDto(
    [property: Description("Identificador del vehículo.")] Guid Id,
    [property: Description("Placa normalizada, sin espacios y en mayúsculas.")] string Plate,
    [property: Description("Kilometraje actual. Solo aumenta cuando una lectura lo supera.")] int CurrentMileage);
