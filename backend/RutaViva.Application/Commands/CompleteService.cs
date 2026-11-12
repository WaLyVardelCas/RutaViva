namespace RutaViva.Application;

public sealed record CompleteService(Guid OrderId, int ActualMileage, DateOnly ActualDate) : ICommand<OrderDto>;
