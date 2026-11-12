using RutaViva.Domain;

namespace RutaViva.Application;

public interface IVehicleRepository
{
    Vehicle? Get(Guid id);
    Vehicle? GetByPlate(Plate plate);
    IReadOnlyList<Vehicle> List();
    void Save(Vehicle vehicle);
}
