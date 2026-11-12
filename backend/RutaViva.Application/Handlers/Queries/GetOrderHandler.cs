using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class GetOrderHandler(IOrderRepository orders)
    : IQueryHandler<GetOrder, OrderDto>
{
    public OrderDto Execute(GetOrder query) =>
        orders.Get(query.Id) is { } order
            ? OrderMapper.From(order)
            : throw new NotFoundException(DomainConstants.OrderNotFound);
}
