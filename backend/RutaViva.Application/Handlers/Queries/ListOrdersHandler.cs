using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class ListOrdersHandler(IOrderRepository orders)
    : IQueryHandler<ListOrders, IReadOnlyList<OrderDto>>
{
    public IReadOnlyList<OrderDto> Execute(ListOrders query) =>
        orders.List(query.Status, query.VehicleId).Select(OrderMapper.From).ToList();
}
