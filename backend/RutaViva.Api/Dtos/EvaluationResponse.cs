using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Resultado de la evaluación diaria simulada.</summary>
public sealed record EvaluationResponse(
    [property: Description("Rutinas cuyo estado cambió en esta ejecución.")] int RoutinesCambiadas,
    [property: Description("Vehículos que la evaluación no pudo completar. El resto sí se procesó.")] IReadOnlyList<Guid> VehiclesFallidos);
