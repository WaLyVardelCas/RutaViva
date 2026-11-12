namespace RutaViva.Job;

public static class JobConstants
{
    public const string EvaluationEnabledKey = "DailyEvaluation:Enabled";
    public const string EvaluationIntervalHoursKey = "DailyEvaluation:IntervalHours";
    public const bool EvaluationEnabledByDefault = true;
    public const int DefaultEvaluationIntervalHours = 24;
    public const string VehicleFailedMessage = "The daily evaluation could not complete vehicle {VehicleId}.";
}
