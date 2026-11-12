using System.ComponentModel;
using RutaViva.Domain;

namespace RutaViva.Application;

public sealed record RoutineDto(
    [property: Description("Identificador de la rutina.")] Guid Id,
    [property: Description("Nombre de la rutina.")] string Name,
    [property: Description("Intervalo en kilómetros. Nulo si la rutina solo vence por tiempo.")] int? Kilometers,
    [property: Description("Intervalo en meses calendario. Nulo si la rutina solo vence por kilometraje.")] int? Months,
    [property: Description("Número del ciclo vigente.")] int CycleNumber,
    [property: Description("Kilometraje de referencia del ciclo vigente.")] int ReferenceMileage,
    [property: Description("Fecha de referencia del ciclo vigente.")] DateOnly ReferenceDate,
    [property: Description("Kilometraje en el que la rutina vence. Nulo si no tiene criterio de kilometraje.")] int? MileageLimit,
    [property: Description("Fecha en la que la rutina vence. Nula si no tiene criterio de tiempo.")] DateOnly? DateLimit,
    [property: Description("UpToDate, DueSoon u Overdue. No retrocede dentro del ciclo.")] RoutineStatus Status);
