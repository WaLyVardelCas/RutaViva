using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class CompleteServiceHandler(
    IOrderRepository orders,
    IVehicleRepository vehicles,
    IDomainEventDispatcher dispatcher)
    : ICommandHandler<CompleteService, OrderDto>
{
    public OrderDto Execute(CompleteService command) =>
        OrderUpdate.Modify(
            orders,
            dispatcher,
            command.OrderId,
            order =>
            {
                Vehicle vehicle = vehicles.Get(order.VehicleId)
                    ?? throw new NotFoundException(DomainConstants.VehicleNotFound);
                OrderValidator.RequireNotBelowVehicle(command.ActualMileage, vehicle.CurrentMileage.Value);
                order.Complete(command.ActualMileage, command.ActualDate);
            });
}
