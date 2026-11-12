using RutaViva.Domain;

namespace RutaViva.Application;

public static class ReadingValidator
{
    public static void RequireEventId(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new BusinessRuleException("El identificador de evento es obligatorio.");
    }
}
