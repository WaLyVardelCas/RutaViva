using Microsoft.Extensions.Logging;
using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Infrastructure;

public sealed class EventLog(ILogger<EventLog> logger) : IEventPublisher
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly Lock _lock = new();

    public void Publish(IDomainEvent domainEvent)
    {
        lock (_lock)
            _domainEvents.Add(domainEvent);
        logger.LogInformation("Domain event {DomainEvent}", domainEvent.GetType().Name);
    }

    public IReadOnlyList<IDomainEvent> List()
    {
        lock (_lock)
            return _domainEvents.ToList();
    }
}
