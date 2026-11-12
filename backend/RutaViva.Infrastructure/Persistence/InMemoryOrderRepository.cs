using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Infrastructure;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, ServiceOrder> _byId = [];
    private readonly Lock _lock = new();

    public ServiceOrder? Get(Guid id)
    {
        lock (_lock)
            return _byId.TryGetValue(id, out ServiceOrder? order) ? Clone(order) : null;
    }

    public IReadOnlyList<ServiceOrder> List(OrderStatus? status, Guid? vehicleId)
    {
        lock (_lock)
            return _byId.Values
                .Where(o => status is null || o.Status == status)
                .Where(o => vehicleId is null || o.VehicleId == vehicleId)
                .Select(Clone)
                .ToList();
    }

    public void Add(ServiceOrder order)
    {
        lock (_lock)
        {
            PersistenceValidator.RequireNoOpenOrder(
                _byId.Values.Any(o => o.VehicleId == order.VehicleId && o.RoutineId == order.RoutineId && o.IsOpen));

            order.SetVersion(DomainConstants.FirstStoredVersion);
            _byId[order.Id] = Clone(order);
        }
    }

    public void Save(ServiceOrder order)
    {
        lock (_lock)
        {
            if (!_byId.TryGetValue(order.Id, out ServiceOrder? almacenada))
                throw new ConcurrencyConflictException();

            PersistenceValidator.RequireVersion(almacenada.Version, order.Version);
            order.SetVersion(almacenada.Version + DomainConstants.VersionStep);
            _byId[order.Id] = Clone(order);
        }
    }

    private static ServiceOrder Clone(ServiceOrder source) =>
        new(
            source.Id,
            source.VehicleId,
            source.RoutineId,
            source.Reference,
            source.Status,
            source.Quote,
            source.AutomaticApproval,
            source.Execution,
            source.Version);
}
