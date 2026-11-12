using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class CreateOrderWhenRoutineOverdue(IOrderRepository orders, IEventPublisher publisher) : IDomainEventHandler
{
    public void Handle(IDomainEvent domainEvent)
    {
        if (domainEvent is not RoutineOverdue overdue)
            return;

        ServiceOrder order = ServiceOrder.Create(
            overdue.VehicleId,
            overdue.RoutineId,
            new CycleReference(overdue.CycleNumber, overdue.ReferenceMileage, overdue.ReferenceDate));

        try
        {
            orders.Add(order);
        }
        catch (DuplicateOpenOrderException)
        {
            return;
        }

        foreach (IDomainEvent raised in order.ConsumeEvents())
            publisher.Publish(raised);
    }
}
