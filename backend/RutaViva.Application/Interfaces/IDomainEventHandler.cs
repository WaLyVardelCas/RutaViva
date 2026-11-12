using RutaViva.Domain;

namespace RutaViva.Application;

public interface IDomainEventHandler
{
    void Handle(IDomainEvent domainEvent);
}
