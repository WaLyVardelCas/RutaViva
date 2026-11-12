namespace RutaViva.Application;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("La copia del agregado no coincide con la almacenada. El cambio no se aplicó.") { }
}
