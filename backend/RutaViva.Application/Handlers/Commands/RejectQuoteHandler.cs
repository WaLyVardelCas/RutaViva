namespace RutaViva.Application;

public sealed class RejectQuoteHandler(IOrderRepository orders, IDomainEventDispatcher dispatcher)
    : ICommandHandler<RejectQuote, OrderDto>
{
    public OrderDto Execute(RejectQuote command) =>
        OrderUpdate.Modify(orders, dispatcher, command.OrderId, order => order.Reject());
}
