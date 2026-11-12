namespace RutaViva.Domain;

public sealed class MileageCriterion : ICriterionStrategy
{
    public static readonly MileageCriterion Instance = new();

    public bool Matches(MaintenanceInterval interval, CriterionSignal signal) =>
        signal is MileageSignal && interval.Kilometers is not null;

    public RoutineStatus Evaluate(MaintenanceCycle cycle, MaintenanceInterval interval, CriterionSignal signal)
    {
        if (signal is not MileageSignal mileage || interval.Kilometers is not int kilometers)
            throw new InvalidOperationException("The mileage criterion needs a mileage interval and a mileage signal.");

        int limit = cycle.ReferenceMileage + kilometers;
        if (mileage.Current >= limit)
            return RoutineStatus.Overdue;

        int remaining = limit - mileage.Current;
        if (remaining <= kilometers * DomainConstants.DueSoonRemainingFraction)
            return RoutineStatus.DueSoon;

        return RoutineStatus.UpToDate;
    }

    public static int? Limit(MaintenanceCycle cycle, MaintenanceInterval interval) =>
        interval.Kilometers is int kilometers ? cycle.ReferenceMileage + kilometers : null;
}
