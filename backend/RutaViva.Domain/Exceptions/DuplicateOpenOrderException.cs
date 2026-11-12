namespace RutaViva.Domain;

public sealed class DuplicateOpenOrderException : DomainException
{
    public DuplicateOpenOrderException() : base("Ya existe una orden abierta para esta rutina.") { }
}
