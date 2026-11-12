namespace RutaViva.Domain;

public static class IntervalValidator
{
    public static void Require(int? kilometers, int? months)
    {
        if (kilometers is null && months is null)
            throw new BusinessRuleException("La rutina necesita un intervalo de kilometraje, de tiempo o de ambos.");
        if (kilometers is <= 0)
            throw new BusinessRuleException("El intervalo de kilometraje debe ser mayor que cero.");
        if (months is <= 0)
            throw new BusinessRuleException("El intervalo de tiempo debe ser mayor que cero.");
    }
}
