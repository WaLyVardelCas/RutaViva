namespace RutaViva.Application;

public sealed class SubmitQuoteHandler(
    IOrderRepository orders,
    IDomainEventDispatcher dispatcher,
    TimeProvider clock)
    : ICommandHandler<SubmitQuote, OrderDto>
{
    public OrderDto Execute(SubmitQuote command) =>
        OrderUpdate.Modify(orders, dispatcher, command.OrderId, order => order.SubmitQuote(command.Amount, Clock.Today(clock)));
}
