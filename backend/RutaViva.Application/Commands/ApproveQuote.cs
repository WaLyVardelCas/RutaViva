namespace RutaViva.Application;

public sealed record ApproveQuote(Guid OrderId) : ICommand<OrderDto>;
