using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class RegisterMileageReadingHandler(
    IVehicleRepository vehicles,
    ITelemetryReadingRegistry registry,
    IDomainEventDispatcher dispatcher)
    : ICommandHandler<RegisterMileageReading, ReadingResult>
{
    public ReadingResult Execute(RegisterMileageReading command)
    {
        ReadingValidator.RequireEventId(command.EventId);

        while (true)
        {
            if (registry.TryGet(command.EventId, out ReadingResult previous))
                return previous;
            if (registry.TryReserve(command.EventId))
                break;
            Thread.Sleep(ApplicationConstants.ReservationWaitMilliseconds);
        }

        try
        {
            List<IDomainEvent> domainEvents = new List<IDomainEvent>();
            ReadingResult result = ReadingResult.Ignored;

            Retry.Execute(() =>
            {
                domainEvents.Clear();
                Vehicle vehicle = vehicles.GetByPlate(new Plate(command.Plate))
                    ?? throw new NotFoundException($"No existe un vehículo con placa {command.Plate}.");

                if (!vehicle.RegisterReading(command.Mileage))
                {
                    result = ReadingResult.Ignored;
                    return;
                }

                vehicles.Save(vehicle);
                domainEvents.AddRange(vehicle.ConsumeEvents());
                result = ReadingResult.Accepted;
            });

            dispatcher.Dispatch(domainEvents);
            registry.Confirm(command.EventId, result);
            return result;
        }
        catch
        {
            registry.Release(command.EventId);
            throw;
        }
    }
}
