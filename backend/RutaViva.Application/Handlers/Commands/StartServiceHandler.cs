namespace RutaViva.Application;

public sealed class StartServiceHandler(IOrderRepository orders, IDomainEventDispatcher dispatcher)
    : ICommandHandler<StartService, OrderDto>
{
    public OrderDto Execute(StartService command) =>
        OrderUpdate.Modify(orders, dispatcher, command.OrderId, order => order.Start());
}
