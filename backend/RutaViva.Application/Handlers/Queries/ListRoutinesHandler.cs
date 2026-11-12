using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class ListRoutinesHandler(IVehicleRepository vehicles)
    : IQueryHandler<ListRoutines, IReadOnlyList<RoutineDto>>
{
    public IReadOnlyList<RoutineDto> Execute(ListRoutines query)
    {
        Vehicle vehicle = vehicles.Get(query.VehicleId)
            ?? throw new NotFoundException(DomainConstants.VehicleNotFound);
        return vehicle.Routines.Select(RoutineMapper.From).ToList();
    }
}
