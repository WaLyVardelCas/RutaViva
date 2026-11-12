namespace RutaViva.Application;

public sealed record CancelOrder(Guid OrderId) : ICommand<OrderDto>;
