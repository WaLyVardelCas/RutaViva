using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class GetVehicleHandler(IVehicleRepository vehicles)
    : IQueryHandler<GetVehicle, VehicleDto>
{
    public VehicleDto Execute(GetVehicle query) =>
        vehicles.Get(query.Id) is { } vehicle
            ? VehicleMapper.From(vehicle)
            : throw new NotFoundException(DomainConstants.VehicleNotFound);
}
