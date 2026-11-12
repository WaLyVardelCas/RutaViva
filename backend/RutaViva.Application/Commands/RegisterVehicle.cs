namespace RutaViva.Application;

public sealed record RegisterVehicle(string Plate, int Mileage, DateOnly RegisteredOn) : ICommand<VehicleDto>;
