namespace RutaViva.Domain;

public sealed record BaseRoutine(string Name, int Kilometers, int Months);

public static class BaseRoutines
{
    public static readonly IReadOnlyList<BaseRoutine> All =
    [
        new(DomainConstants.OilChange, DomainConstants.OilChangeKilometers, DomainConstants.OilChangeMonths),
        new(DomainConstants.BrakeReview, DomainConstants.BrakeReviewKilometers, DomainConstants.BrakeReviewMonths),
        new(DomainConstants.TireChange, DomainConstants.TireChangeKilometers, DomainConstants.TireChangeMonths)
    ];
}
