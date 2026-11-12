namespace RutaViva.Domain;

public static class RoutineValidator
{
    public static string RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException(DomainConstants.RoutineNameRequired);
        return name.Trim();
    }

    public static void RequireUniqueName(IEnumerable<string> existingNames, string name)
    {
        if (existingNames.Any(current => string.Equals(current, name, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException($"El vehículo ya tiene una rutina llamada \"{name}\".");
    }

    public static void RequireRestart(int actualMileage, int referenceMileage, DateOnly actualDate, DateOnly referenceDate)
    {
        if (actualMileage < referenceMileage)
            throw new BusinessRuleException(DomainConstants.MileageBeforeReference);
        if (actualDate < referenceDate)
            throw new BusinessRuleException(DomainConstants.DateBeforeReference);
    }
}
