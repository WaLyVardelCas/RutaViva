using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Infrastructure;

public static class PersistenceValidator
{
    public static void RequireVersion(int almacenada, int esperada)
    {
        if (almacenada != esperada)
            throw new ConcurrencyConflictException();
    }

    public static void RequireNewAggregate(int version)
    {
        if (version != DomainConstants.UnstoredVersion)
            throw new ConcurrencyConflictException();
    }

    public static void RequireAvailablePlate(bool taken, string plate)
    {
        if (taken)
            throw new BusinessRuleException($"Ya existe un vehículo con placa {plate}.");
    }

    public static void RequireNoOpenOrder(bool existeAbierta)
    {
        if (existeAbierta)
            throw new DuplicateOpenOrderException();
    }
}
