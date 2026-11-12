namespace RutaViva.Application;

public sealed record ListRoutines(Guid VehicleId) : IQuery<IReadOnlyList<RoutineDto>>;
