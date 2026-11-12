namespace RutaViva.Application;

public sealed record SubmitQuote(Guid OrderId, decimal Amount) : ICommand<OrderDto>;
