using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class AddRoutineHandler(IVehicleRepository vehicles)
    : ICommandHandler<AddRoutine, RoutineDto>
{
    public RoutineDto Execute(AddRoutine command)
    {
        RoutineDto? createdItem = null;
        Retry.Execute(() =>
        {
            Vehicle vehicle = vehicles.Get(command.VehicleId)
                ?? throw new NotFoundException(DomainConstants.VehicleNotFound);
            MaintenanceRoutine routine = vehicle.AddRoutine(command.Name, command.Kilometers, command.Months, command.Date);
            vehicles.Save(vehicle);
            createdItem = RoutineMapper.From(routine);
        });
        return createdItem!;
    }
}
