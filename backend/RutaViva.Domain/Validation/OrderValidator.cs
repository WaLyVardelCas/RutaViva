namespace RutaViva.Domain;

public static class OrderValidator
{
    public static void RequireStatus(OrderStatus current, OrderStatus expected, string message)
    {
        if (current != expected)
            throw new BusinessRuleException(message);
    }

    public static void RequireNoExistingQuote(bool alreadyHasQuote)
    {
        if (alreadyHasQuote)
            throw new BusinessRuleException("La cotización ya fue enviada y no se puede modificar.");
    }

    public static void RequireCancelable(OrderStatus status)
    {
        if (status is not (OrderStatus.Created or OrderStatus.Quoted or OrderStatus.Approved))
            throw new BusinessRuleException("La orden solo se puede cancelar antes de iniciar el servicio.");
    }

    public static void RequireCompletion(int actualMileage, int referenceMileage, DateOnly actualDate, DateOnly referenceDate)
    {
        MileageValidator.RequireNonNegative(actualMileage, "El kilometraje real no puede ser negativo.");
        if (actualMileage < referenceMileage)
            throw new BusinessRuleException(DomainConstants.MileageBeforeReference);
        if (actualDate < referenceDate)
            throw new BusinessRuleException(DomainConstants.DateBeforeReference);
    }
}
