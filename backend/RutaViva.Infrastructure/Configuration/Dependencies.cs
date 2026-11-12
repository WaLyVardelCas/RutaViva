using Microsoft.Extensions.DependencyInjection;
using RutaViva.Application;

namespace RutaViva.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection AddRutaViva(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<ITelemetryReadingRegistry, InMemoryReadingRegistry>();
        services.AddSingleton<IEventPublisher, EventLog>();
        services.AddCqrs();

        services.AddSingleton<IDomainEventHandler, CreateOrderWhenRoutineOverdue>();
        services.AddSingleton<IDomainEventHandler, RestartCycleWhenOrderCompleted>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }

    private static void AddCqrs(this IServiceCollection services)
    {
        foreach (Type type in typeof(IMediator).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (Type contract in type.GetInterfaces())
            {
                if (!contract.IsGenericType)
                    continue;

                Type definition = contract.GetGenericTypeDefinition();
                if (definition == typeof(ICommandHandler<,>) || definition == typeof(IQueryHandler<,>))
                    services.AddSingleton(contract, type);
            }
        }

        services.AddSingleton<IMediator, Mediator>();
    }
}
