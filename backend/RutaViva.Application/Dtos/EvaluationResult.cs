namespace RutaViva.Application;

public sealed record EvaluationResult(int ChangedRoutines, IReadOnlyList<Guid> FailedVehicles);
