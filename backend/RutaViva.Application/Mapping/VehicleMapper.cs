using RutaViva.Domain;

namespace RutaViva.Application;

internal static class VehicleMapper
{
    public static VehicleDto From(Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Plate.Value, vehicle.CurrentMileage.Value);
}
