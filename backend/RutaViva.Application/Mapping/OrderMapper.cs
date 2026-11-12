using RutaViva.Domain;

namespace RutaViva.Application;

internal static class OrderMapper
{
    public static OrderDto From(ServiceOrder order) =>
        new(
            order.Id,
            order.VehicleId,
            order.RoutineId,
            order.Status,
            order.Quote?.Amount.Amount,
            order.AutomaticApproval,
            order.Execution?.ActualMileage,
            order.Execution?.ActualDate);
}
