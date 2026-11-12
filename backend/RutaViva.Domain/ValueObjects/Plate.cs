namespace RutaViva.Domain;

public readonly record struct Plate
{
    public string Value { get; }

    public Plate(string value) => Value = PlateValidator.Normalize(value);

    public override string ToString() => Value;
}
