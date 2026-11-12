namespace RutaViva.Application;

public sealed class ApproveQuoteHandler(IOrderRepository orders, IDomainEventDispatcher dispatcher)
    : ICommandHandler<ApproveQuote, OrderDto>
{
    public OrderDto Execute(ApproveQuote command) =>
        OrderUpdate.Modify(orders, dispatcher, command.OrderId, order => order.Approve());
}
