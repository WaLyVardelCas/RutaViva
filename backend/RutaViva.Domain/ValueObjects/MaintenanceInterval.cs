namespace RutaViva.Domain;

public sealed record MaintenanceInterval
{
    public int? Kilometers { get; }
    public int? Months { get; }

    public MaintenanceInterval(int? kilometers, int? months)
    {
        IntervalValidator.Require(kilometers, months);
        Kilometers = kilometers;
        Months = months;
    }
}
