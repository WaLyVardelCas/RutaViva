using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class RestartCycleWhenOrderCompleted(IVehicleRepository vehicles, IEventPublisher publisher) : IDomainEventHandler
{
    public void Handle(IDomainEvent domainEvent)
    {
        if (domainEvent is not ServiceOrderCompleted completedOrder)
            return;

        List<IDomainEvent> domainEvents = new List<IDomainEvent>();
        Retry.Execute(() =>
        {
            domainEvents.Clear();
            Vehicle? vehicle = vehicles.Get(completedOrder.VehicleId);
            if (vehicle is null)
                return;

            if (!vehicle.RestartCycle(
                    completedOrder.RoutineId,
                    completedOrder.CycleNumber,
                    completedOrder.ActualMileage,
                    completedOrder.ActualDate))
                return;

            vehicles.Save(vehicle);
            domainEvents.AddRange(vehicle.ConsumeEvents());
        });

        foreach (IDomainEvent raised in domainEvents)
            publisher.Publish(raised);
    }
}
