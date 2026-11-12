namespace RutaViva.Domain;

public static class MoneyValidator
{
    public static void RequirePositive(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("El monto de la cotización debe ser mayor que cero.");
    }
}
