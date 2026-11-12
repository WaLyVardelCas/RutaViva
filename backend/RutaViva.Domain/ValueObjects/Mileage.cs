namespace RutaViva.Domain;

public readonly record struct Mileage
{
    public int Value { get; }

    public Mileage(int value)
    {
        MileageValidator.RequireNonNegative(value);
        Value = value;
    }

    public bool IsGreaterThan(Mileage other) => Value > other.Value;

    public override string ToString() => Value.ToString();
}
