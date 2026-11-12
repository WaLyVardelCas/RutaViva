namespace RutaViva.Domain;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
