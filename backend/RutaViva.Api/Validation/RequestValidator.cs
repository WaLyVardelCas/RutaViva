using RutaViva.Domain;

namespace RutaViva.Api;

public static class RequestValidator
{
    public static string? Vehicle(VehicleRequest? request) =>
        request is null || string.IsNullOrWhiteSpace(request.Plate) || request.Mileage is null
            ? "La placa y el kilometraje inicial son obligatorios."
            : null;

    public static string? Routine(RoutineRequest? request) =>
        request is null || string.IsNullOrWhiteSpace(request.Name)
            ? "La rutina necesita un nombre."
            : null;

    public static string? Reading(TelemetryReadingRequest? reading) =>
        reading is null
        || string.IsNullOrWhiteSpace(reading.EventId)
        || string.IsNullOrWhiteSpace(reading.Plate)
        || reading.Mileage is null
        || reading.ReadingDate is null
            ? "La lectura requiere identificador de evento, placa, kilometraje y fecha."
            : null;

    public static string? Status(string? status, out OrderStatus? filter)
    {
        filter = null;
        if (string.IsNullOrWhiteSpace(status))
            return null;
        if (!Enum.TryParse(status, ignoreCase: true, out OrderStatus value))
            return $"El estado \"{status}\" no existe.";
        filter = value;
        return null;
    }

    public static string? Quote(QuoteRequest? request) =>
        request?.Amount is null ? "El monto de la cotización es obligatorio." : null;

    public static string? Completion(CompleteServiceRequest? request) =>
        request?.Mileage is null || request.Date is null
            ? "Completar el servicio requiere el kilometraje y la fecha reales."
            : null;

    public static string? Evaluation(EvaluationRequest? request) =>
        request?.Date is null || request.Date == default
            ? "La fecha de la evaluación es obligatoria."
            : null;
}
