using RutaViva.Domain;

namespace RutaViva.Application;

internal static class RoutineMapper
{
    public static RoutineDto From(MaintenanceRoutine routine) =>
        new(
            routine.Id,
            routine.Name,
            routine.Interval.Kilometers,
            routine.Interval.Months,
            routine.Cycle.Number,
            routine.Cycle.ReferenceMileage,
            routine.Cycle.ReferenceDate,
            routine.MileageLimit,
            routine.DateLimit,
            routine.Status);
}
