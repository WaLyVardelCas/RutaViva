namespace RutaViva.Domain;

public abstract record CriterionSignal;

public sealed record MileageSignal(int Current) : CriterionSignal;

public sealed record TimeSignal(DateOnly Today) : CriterionSignal;
