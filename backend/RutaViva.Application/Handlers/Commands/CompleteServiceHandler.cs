namespace RutaViva.Application;

public sealed class CompleteServiceHandler(IOrderRepository orders, IDomainEventDispatcher dispatcher)
    : ICommandHandler<CompleteService, OrderDto>
{
    public OrderDto Execute(CompleteService command) =>
        OrderUpdate.Modify(
            orders,
            dispatcher,
            command.OrderId,
            order => order.Complete(command.ActualMileage, command.ActualDate));
}
