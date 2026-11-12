using RutaViva.Domain;

namespace RutaViva.Application;

public interface IEventPublisher
{
    void Publish(IDomainEvent domainEvent);
    IReadOnlyList<IDomainEvent> List();
}
