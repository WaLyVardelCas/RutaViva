using RutaViva.Application;

namespace RutaViva.Infrastructure;

public sealed class InMemoryReadingRegistry : ITelemetryReadingRegistry
{
    private readonly Dictionary<string, ReadingResult> _processed = new(StringComparer.Ordinal);
    private readonly HashSet<string> _inProgress = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();

    public bool TryGet(string eventId, out ReadingResult result)
    {
        lock (_lock)
            return _processed.TryGetValue(eventId, out result);
    }

    public bool TryReserve(string eventId)
    {
        lock (_lock)
        {
            if (_processed.ContainsKey(eventId) || !_inProgress.Add(eventId))
                return false;
            return true;
        }
    }

    public void Confirm(string eventId, ReadingResult result)
    {
        lock (_lock)
        {
            _processed[eventId] = result;
            _inProgress.Remove(eventId);
        }
    }

    public void Release(string eventId)
    {
        lock (_lock)
            _inProgress.Remove(eventId);
    }
}
