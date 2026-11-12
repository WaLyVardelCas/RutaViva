namespace RutaViva.Domain;

public static class DomainConstants
{
    public const int UnstoredVersion = 0;
    public const int FirstStoredVersion = 1;
    public const int VersionStep = 1;

    public const int FirstCycle = 1;
    public const int CycleStep = 1;

    public const decimal DueSoonRemainingFraction = 0.10m;
    public const decimal ClientApprovalThreshold = 1_500_000m;

    public const string OilChange = "Cambio de aceite";
    public const int OilChangeKilometers = 10_000;
    public const int OilChangeMonths = 6;
    public const string BrakeReview = "Revisión de frenos";
    public const int BrakeReviewKilometers = 20_000;
    public const int BrakeReviewMonths = 12;
    public const string TireChange = "Cambio de llantas";
    public const int TireChangeKilometers = 40_000;
    public const int TireChangeMonths = 24;

    public const string VehicleNotFound = "El vehículo no existe.";
    public const string OrderNotFound = "La orden no existe.";
    public const string RoutineNameRequired = "La rutina necesita un nombre.";
    public const string MileageBeforeReference = "El kilometraje real no puede ser anterior a la referencia del ciclo.";
    public const string MileageBelowVehicle = "El kilometraje real no puede ser menor al kilometraje actual del vehículo";
    public const string DateBeforeReference = "La fecha real no puede ser anterior a la referencia del ciclo.";
}
