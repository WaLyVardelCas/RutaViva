using RutaViva.Domain;

namespace RutaViva.Application;

public interface IOrderRepository
{
    ServiceOrder? Get(Guid id);
    IReadOnlyList<ServiceOrder> List(OrderStatus? status, Guid? vehicleId);
    void Add(ServiceOrder order);
    void Save(ServiceOrder order);
}
