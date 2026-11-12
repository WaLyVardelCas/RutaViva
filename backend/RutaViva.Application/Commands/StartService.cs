namespace RutaViva.Application;

public sealed record StartService(Guid OrderId) : ICommand<OrderDto>;
