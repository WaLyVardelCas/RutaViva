namespace RutaViva.Application;

public sealed record RegisterMileageReading(string EventId, string Plate, int Mileage)
    : ICommand<ReadingResult>;
