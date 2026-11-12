using RutaViva.Domain;

namespace RutaViva.Application;

internal static class OrderUpdate
{
    public static OrderDto Modify(
        IOrderRepository orders,
        IDomainEventDispatcher dispatcher,
        Guid orderId,
        Action<ServiceOrder> change)
    {
        OrderDto? dto = null;
        List<IDomainEvent> domainEvents = new List<IDomainEvent>();
        Retry.Execute(() =>
        {
            domainEvents.Clear();
            ServiceOrder order = orders.Get(orderId)
                ?? throw new NotFoundException(DomainConstants.OrderNotFound);
            change(order);
            orders.Save(order);
            domainEvents.AddRange(order.ConsumeEvents());
            dto = OrderMapper.From(order);
        });
        dispatcher.Dispatch(domainEvents);
        return dto!;
    }
}
