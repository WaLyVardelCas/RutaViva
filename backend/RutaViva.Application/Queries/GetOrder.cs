namespace RutaViva.Application;

public sealed record GetOrder(Guid Id) : IQuery<OrderDto>;
