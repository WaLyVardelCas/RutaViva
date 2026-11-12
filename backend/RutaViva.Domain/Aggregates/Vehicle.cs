namespace RutaViva.Domain;

public sealed class Vehicle
{
    private readonly List<MaintenanceRoutine> _routines = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; }
    public Plate Plate { get; }
    public Mileage CurrentMileage { get; private set; }
    public int Version { get; private set; }
    public IReadOnlyList<MaintenanceRoutine> Routines => _routines;

    public Vehicle(Guid id, Plate plate, Mileage currentMileage, int version, IEnumerable<MaintenanceRoutine> routines)
    {
        Id = id;
        Plate = plate;
        CurrentMileage = currentMileage;
        Version = version;
        _routines.AddRange(routines);
    }

    public static Vehicle Register(string plate, int initialMileage, DateOnly registeredOn)
    {
        Vehicle vehicle = new Vehicle(Guid.NewGuid(), new Plate(plate), new Mileage(initialMileage), DomainConstants.UnstoredVersion, []);
        foreach (BaseRoutine routine in BaseRoutines.All)
            vehicle.AddRoutine(routine.Name, routine.Kilometers, routine.Months, registeredOn);
        return vehicle;
    }

    public bool RegisterReading(int mileage)
    {
        Mileage reading = new Mileage(mileage);
        if (!reading.IsGreaterThan(CurrentMileage))
            return false;

        CurrentMileage = reading;
        foreach (MaintenanceRoutine routine in _routines)
            routine.Evaluate(Id, new MileageSignal(reading.Value));
        return true;
    }

    public int EvaluateByTime(DateOnly today)
    {
        int changes = 0;
        foreach (MaintenanceRoutine routine in _routines)
        {
            RoutineStatus antes = routine.Status;
            routine.Evaluate(Id, new TimeSignal(today));
            if (routine.Status != antes)
                changes++;
        }
        return changes;
    }

    public MaintenanceRoutine AddRoutine(string name, int? kilometers, int? months, DateOnly configuredOn)
    {
        string nameLimpio = name?.Trim() ?? "";
        RoutineValidator.RequireUniqueName(_routines.Select(r => r.Name), nameLimpio);

        MaintenanceRoutine routine = MaintenanceRoutine.Create(
            nameLimpio,
            new MaintenanceInterval(kilometers, months),
            CurrentMileage.Value,
            configuredOn);
        _routines.Add(routine);
        return routine;
    }

    public bool RestartCycle(Guid routineId, int cycleNumber, int actualMileage, DateOnly actualDate)
    {
        MaintenanceRoutine? routine = _routines.FirstOrDefault(r => r.Id == routineId);
        if (routine is null || routine.Cycle.Number != cycleNumber)
            return false;

        routine.Restart(actualMileage, actualDate);
        _domainEvents.Add(new RoutineCycleRestarted(Id, routineId, routine.Cycle.Number));
        RegisterReading(actualMileage);
        return true;
    }

    public IReadOnlyList<IDomainEvent> ConsumeEvents()
    {
        List<IDomainEvent> domainEvents = new List<IDomainEvent>(_domainEvents);
        _domainEvents.Clear();
        foreach (MaintenanceRoutine routine in _routines)
        {
            domainEvents.AddRange(routine.DomainEvents);
            routine.ConsumeEvents();
        }
        return domainEvents;
    }

    public void SetVersion(int version) => Version = version;
}
