using RutaViva.Domain;

namespace RutaViva.Application;

public interface IDomainEventDispatcher
{
    void Dispatch(IEnumerable<IDomainEvent> domainEvents);
}
