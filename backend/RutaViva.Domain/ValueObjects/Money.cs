namespace RutaViva.Domain;

public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        MoneyValidator.RequirePositive(amount);
        Amount = amount;
    }

    public bool RequiresClientApproval => Amount > DomainConstants.ClientApprovalThreshold;
}
