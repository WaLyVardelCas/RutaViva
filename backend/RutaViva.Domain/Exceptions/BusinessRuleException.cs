namespace RutaViva.Domain;

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}
