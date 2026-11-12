namespace RutaViva.Application;

public interface ITelemetryReadingRegistry
{
    bool TryGet(string eventId, out ReadingResult result);
    bool TryReserve(string eventId);
    void Confirm(string eventId, ReadingResult result);
    void Release(string eventId);
}
