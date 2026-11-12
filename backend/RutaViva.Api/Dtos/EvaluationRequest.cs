using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Fecha simulada para la evaluación diaria. Operación de demostración.</summary>
public sealed record EvaluationRequest(
    [property: Description("Fecha a evaluar, YYYY-MM-DD. Obligatoria. No usa el reloj del servidor.")] DateOnly? Date);
