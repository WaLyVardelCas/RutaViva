using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Alta de un vehículo.</summary>
public sealed record VehicleRequest(
    [property: Description("Placa. Obligatoria. Se guarda sin espacios y en mayúsculas.")] string? Plate,
    [property: Description("Kilometraje inicial. Obligatorio y no negativo.")] int? Mileage);
