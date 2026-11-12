namespace RutaViva.Domain;

public sealed class TimeCriterion : ICriterionStrategy
{
    public static readonly TimeCriterion Instance = new();

    public bool Matches(MaintenanceInterval interval, CriterionSignal signal) =>
        signal is TimeSignal && interval.Months is not null;

    public RoutineStatus Evaluate(MaintenanceCycle cycle, MaintenanceInterval interval, CriterionSignal signal)
    {
        if (signal is not TimeSignal time || interval.Months is not int months)
            throw new InvalidOperationException("The time criterion needs a time interval and a time signal.");

        DateOnly limit = Limit(cycle.ReferenceDate, months);
        if (time.Today >= limit)
            return RoutineStatus.Overdue;

        int totalDays = limit.DayNumber - cycle.ReferenceDate.DayNumber;
        int remainingDays = limit.DayNumber - time.Today.DayNumber;
        if (remainingDays <= totalDays * DomainConstants.DueSoonRemainingFraction)
            return RoutineStatus.DueSoon;

        return RoutineStatus.UpToDate;
    }

    public static DateOnly? Limit(MaintenanceCycle cycle, MaintenanceInterval interval) =>
        interval.Months is int months ? Limit(cycle.ReferenceDate, months) : null;

    public static DateOnly Limit(DateOnly reference, int months) => reference.AddMonths(months);
}
