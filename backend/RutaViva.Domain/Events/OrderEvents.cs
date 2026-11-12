namespace RutaViva.Domain;

public sealed record ServiceOrderCreated(Guid OrderId, Guid VehicleId, Guid RoutineId) : IDomainEvent;

public sealed record OrderQuoted(Guid OrderId, decimal Amount) : IDomainEvent;

public sealed record QuoteApproved(Guid OrderId, bool Automatic) : IDomainEvent;

public sealed record QuoteRejected(Guid OrderId) : IDomainEvent;

public sealed record OrderCanceled(Guid OrderId) : IDomainEvent;

public sealed record ServiceStarted(Guid OrderId) : IDomainEvent;

public sealed record ServiceOrderCompleted(
    Guid OrderId,
    Guid VehicleId,
    Guid RoutineId,
    int CycleNumber,
    int ActualMileage,
    DateOnly ActualDate) : IDomainEvent;
