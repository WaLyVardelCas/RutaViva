using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class DomainEventDispatcher(
    IEventPublisher publisher,
    IEnumerable<IDomainEventHandler> handleres) : IDomainEventDispatcher
{
    public void Dispatch(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            publisher.Publish(domainEvent);
            foreach (IDomainEventHandler handler in handleres)
                handler.Handle(domainEvent);
        }
    }
}
