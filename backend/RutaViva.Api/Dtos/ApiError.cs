using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Cuerpo de error de la API. Lo devuelven 400, 404, 409 y 500.</summary>
public sealed record ApiError(
    [property: Description("Categoría: Datos inválidos, No encontrado, Regla de negocio o Error interno.")] string Title,
    [property: Description("Motivo concreto. En un 500 no incluye el detalle interno.")] string? Detail);
