namespace RutaViva.Domain;

public sealed record RoutineDueSoon(Guid VehicleId, Guid RoutineId, int CycleNumber) : IDomainEvent;

public sealed record RoutineOverdue(
    Guid VehicleId,
    Guid RoutineId,
    int CycleNumber,
    int ReferenceMileage,
    DateOnly ReferenceDate) : IDomainEvent;

public sealed record RoutineCycleRestarted(Guid VehicleId, Guid RoutineId, int CycleNumber) : IDomainEvent;
