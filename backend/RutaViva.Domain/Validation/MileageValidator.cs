namespace RutaViva.Domain;

public static class MileageValidator
{
    public static void RequireNonNegative(int value, string message = "El kilometraje no puede ser negativo.")
    {
        if (value < 0)
            throw new BusinessRuleException(message);
    }
}
