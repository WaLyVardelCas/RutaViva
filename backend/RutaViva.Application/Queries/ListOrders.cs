using RutaViva.Domain;

namespace RutaViva.Application;

public sealed record ListOrders(OrderStatus? Status, Guid? VehicleId) : IQuery<IReadOnlyList<OrderDto>>;
