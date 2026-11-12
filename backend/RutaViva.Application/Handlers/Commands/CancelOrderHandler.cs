namespace RutaViva.Application;

public sealed class CancelOrderHandler(IOrderRepository orders, IDomainEventDispatcher dispatcher)
    : ICommandHandler<CancelOrder, OrderDto>
{
    public OrderDto Execute(CancelOrder command) =>
        OrderUpdate.Modify(orders, dispatcher, command.OrderId, order => order.Cancel());
}
