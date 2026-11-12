namespace RutaViva.Application;

public sealed record GetVehicle(Guid Id) : IQuery<VehicleDto>;
