namespace RutaViva.Application;

public sealed record AddRoutine(Guid VehicleId, string Name, int? Kilometers, int? Months, DateOnly Date)
    : ICommand<RoutineDto>;
