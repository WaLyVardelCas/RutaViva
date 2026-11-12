namespace RutaViva.Application;

public sealed class ListVehiclesHandler(IVehicleRepository vehicles)
    : IQueryHandler<ListVehicles, IReadOnlyList<VehicleDto>>
{
    public IReadOnlyList<VehicleDto> Execute(ListVehicles query) =>
        vehicles.List().Select(VehicleMapper.From).ToList();
}
