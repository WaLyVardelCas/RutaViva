namespace RutaViva.Domain;

public sealed record MaintenanceCycle(int Number, int ReferenceMileage, DateOnly ReferenceDate)
{
    public CycleReference ToReference() => new(Number, ReferenceMileage, ReferenceDate);
}

public sealed record CycleReference(int CycleNumber, int ReferenceMileage, DateOnly ReferenceDate);
