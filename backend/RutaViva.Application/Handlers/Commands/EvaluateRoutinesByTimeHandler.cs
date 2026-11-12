using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class EvaluateRoutinesByTimeHandler(
    IVehicleRepository vehicles,
    IDomainEventDispatcher dispatcher)
    : ICommandHandler<EvaluateRoutinesByTime, EvaluationResult>
{
    public EvaluationResult Execute(EvaluateRoutinesByTime command)
    {
        int changedCount = 0;
        List<Guid> failedIds = new List<Guid>();

        foreach (Vehicle summary in vehicles.List())
        {
            try
            {
                int vehicleChanges = 0;
                Retry.Execute(() =>
                {
                    Vehicle vehicle = vehicles.Get(summary.Id)
                        ?? throw new NotFoundException(DomainConstants.VehicleNotFound);
                    int changes = vehicle.EvaluateByTime(command.Date);
                    if (changes == 0)
                    {
                        vehicleChanges = 0;
                        return;
                    }

                    vehicles.Save(vehicle);
                    dispatcher.Dispatch(vehicle.ConsumeEvents());
                    vehicleChanges = changes;
                });
                changedCount += vehicleChanges;
            }
            catch (Exception)
            {
                failedIds.Add(summary.Id);
            }
        }

        return new EvaluationResult(changedCount, failedIds);
    }
}
