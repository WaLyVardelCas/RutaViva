namespace RutaViva.Application;

public sealed record RejectQuote(Guid OrderId) : ICommand<OrderDto>;
