namespace RutaViva.Domain;

public static class PlateValidator
{
    public static string Normalize(string? value)
    {
        string normalizada = new string((value ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (normalizada.Length == 0)
            throw new BusinessRuleException("La placa es obligatoria.");
        return normalizada;
    }
}
