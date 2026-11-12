namespace RutaViva.Domain;

public sealed class MaintenanceRoutine
{
    private static readonly ICriterionStrategy[] Criteria = [MileageCriterion.Instance, TimeCriterion.Instance];

    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; }
    public string Name { get; }
    public MaintenanceInterval Interval { get; }
    public MaintenanceCycle Cycle { get; private set; }
    public RoutineStatus Status { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public int? MileageLimit => MileageCriterion.Limit(Cycle, Interval);

    public DateOnly? DateLimit => TimeCriterion.Limit(Cycle, Interval);

    public MaintenanceRoutine(Guid id, string name, MaintenanceInterval interval, MaintenanceCycle cycle, RoutineStatus status)
    {
        Id = id;
        Name = RoutineValidator.RequireName(name);
        Interval = interval;
        Cycle = cycle;
        Status = status;
    }

    public static MaintenanceRoutine Create(string name, MaintenanceInterval interval, int referenceMileage, DateOnly referenceDate) =>
        new(Guid.NewGuid(), name, interval, new MaintenanceCycle(DomainConstants.FirstCycle, referenceMileage, referenceDate), RoutineStatus.UpToDate);

    public void Evaluate(Guid vehicleId, CriterionSignal signal)
    {
        foreach (ICriterionStrategy criterion in Criteria)
        {
            if (!criterion.Matches(Interval, signal))
                continue;

            Apply(vehicleId, criterion.Evaluate(Cycle, Interval, signal));
        }
    }

    public void Restart(int actualMileage, DateOnly actualDate)
    {
        RoutineValidator.RequireRestart(actualMileage, Cycle.ReferenceMileage, actualDate, Cycle.ReferenceDate);
        Cycle = new MaintenanceCycle(Cycle.Number + DomainConstants.CycleStep, actualMileage, actualDate);
        Status = RoutineStatus.UpToDate;
    }

    public void ConsumeEvents() => _domainEvents.Clear();

    private void Apply(Guid vehicleId, RoutineStatus criterion)
    {
        RoutineStatus nuevo = criterion > Status ? criterion : Status;
        if (nuevo == Status)
            return;

        Status = nuevo;
        if (nuevo == RoutineStatus.DueSoon)
            _domainEvents.Add(new RoutineDueSoon(vehicleId, Id, Cycle.Number));
        else if (nuevo == RoutineStatus.Overdue)
            _domainEvents.Add(new RoutineOverdue(
                vehicleId,
                Id,
                Cycle.Number,
                Cycle.ReferenceMileage,
                Cycle.ReferenceDate));
    }
}
