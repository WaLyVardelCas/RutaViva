using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Infrastructure;

public sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly Dictionary<Guid, Vehicle> _byId = [];
    private readonly Dictionary<string, Guid> _byPlate = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();

    public Vehicle? Get(Guid id)
    {
        lock (_lock)
            return _byId.TryGetValue(id, out Vehicle? vehicle) ? Clone(vehicle) : null;
    }

    public Vehicle? GetByPlate(Plate plate)
    {
        lock (_lock)
            return _byPlate.TryGetValue(plate.Value, out Guid id) ? Clone(_byId[id]) : null;
    }

    public IReadOnlyList<Vehicle> List()
    {
        lock (_lock)
            return _byId.Values.Select(Clone).ToList();
    }

    public void Save(Vehicle vehicle)
    {
        lock (_lock)
        {
            if (_byId.TryGetValue(vehicle.Id, out Vehicle? stored))
            {
                PersistenceValidator.RequireVersion(stored.Version, vehicle.Version);
                vehicle.SetVersion(stored.Version + DomainConstants.VersionStep);
                _byId[vehicle.Id] = Clone(vehicle);
                return;
            }

            PersistenceValidator.RequireNewAggregate(vehicle.Version);
            PersistenceValidator.RequireAvailablePlate(_byPlate.ContainsKey(vehicle.Plate.Value), vehicle.Plate.Value);

            vehicle.SetVersion(DomainConstants.FirstStoredVersion);
            _byId[vehicle.Id] = Clone(vehicle);
            _byPlate[vehicle.Plate.Value] = vehicle.Id;
        }
    }

    private static Vehicle Clone(Vehicle source) =>
        new(
            source.Id,
            source.Plate,
            source.CurrentMileage,
            source.Version,
            source.Routines.Select(r => new MaintenanceRoutine(r.Id, r.Name, r.Interval, r.Cycle, r.Status)));
}
