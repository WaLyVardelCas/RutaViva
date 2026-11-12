using RutaViva.Domain;

namespace RutaViva.Application;

public sealed class RegisterVehicleHandler(IVehicleRepository vehicles)
    : ICommandHandler<RegisterVehicle, VehicleDto>
{
    public VehicleDto Execute(RegisterVehicle command)
    {
        Vehicle vehicle = Vehicle.Register(command.Plate, command.Mileage, command.RegisteredOn);
        vehicles.Save(vehicle);
        return VehicleMapper.From(vehicle);
    }
}
