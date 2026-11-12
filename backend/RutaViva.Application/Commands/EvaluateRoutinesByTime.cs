namespace RutaViva.Application;

public sealed record EvaluateRoutinesByTime(DateOnly Date) : ICommand<EvaluationResult>;
