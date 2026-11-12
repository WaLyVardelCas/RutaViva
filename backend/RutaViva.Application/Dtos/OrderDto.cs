using System.ComponentModel;
using RutaViva.Domain;

namespace RutaViva.Application;

public sealed record OrderDto(
    [property: Description("Identificador de la orden.")] Guid Id,
    [property: Description("Vehículo de la orden.")] Guid VehicleId,
    [property: Description("Rutina que originó la orden.")] Guid RoutineId,
    [property: Description("Created, Quoted, Approved, InProgress, Completed o Canceled.")] OrderStatus Status,
    [property: Description("Monto cotizado en COP. Nulo mientras la orden está Created.")] decimal? Amount,
    [property: Description("True si la aprobación fue automática por no superar COP 1.500.000. Nulo si aún no hay aprobación.")] bool? AutomaticApproval,
    [property: Description("Kilometraje real informado al completar. Nulo hasta entonces.")] int? ActualMileage,
    [property: Description("Fecha real informada al completar. Nula hasta entonces.")] DateOnly? ActualDate);
