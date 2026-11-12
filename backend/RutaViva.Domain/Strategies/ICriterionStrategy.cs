namespace RutaViva.Domain;

public interface ICriterionStrategy
{
    bool Matches(MaintenanceInterval interval, CriterionSignal signal);

    RoutineStatus Evaluate(MaintenanceCycle cycle, MaintenanceInterval interval, CriterionSignal signal);
}
