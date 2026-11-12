using Microsoft.AspNetCore.Mvc;
using RutaViva.Application;

namespace RutaViva.Api;

/// <summary>
/// Operaciones de soporte para la demostración. Simulan la telemetría y disparan la evaluación diaria.
/// El portal no las consume.
/// </summary>
[ApiController]
[Produces("application/json")]
[Tags(OpenApi.SupportTag)]
public sealed class SupportController(IMediator mediator, ILogger<SupportController> logger) : ControllerBase
{
    /// <summary>Demostración. Simula una lectura de telemetría de kilometraje.</summary>
    /// <remarks>
    /// No evalúa el vencimiento por tiempo. Responde 200 tanto si la lectura se aplica como si se ignora;
    /// la diferencia está en <c>result</c>.
    /// </remarks>
    /// <response code="200">Accepted si el kilometraje se aplicó. Ignored si no supera el actual o si el eventId ya fue procesado.</response>
    /// <response code="400">Faltan el identificador, la placa, el kilometraje o la fecha, o el JSON no es válido.</response>
    /// <response code="404">No hay un vehículo con esa placa.</response>
    /// <response code="409">El kilometraje es negativo o no se pudo resolver la concurrencia.</response>
    [HttpPost("/api/telemetry/readings")]
    [ProducesResponseType(typeof(ReadingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<ReadingResponse> Reading(TelemetryReadingRequest? reading)
    {
        if (RequestValidator.Reading(reading) is string error)
            return InvalidRequest.For(error);

        ReadingResult result = mediator.Send(new RegisterMileageReading(
            reading!.EventId!, reading.Plate!, reading.Mileage!.Value));
        return Ok(new ReadingResponse(result));
    }

    /// <summary>Demostración. Ejecuta la evaluación diaria con una fecha simulada.</summary>
    /// <remarks>
    /// Usa la misma operación que el proceso programado, pero la fecha la envía quien llama.
    /// Solo evalúa el criterio de tiempo. Una misma fecha repetida no produce efectos nuevos.
    /// </remarks>
    /// <response code="200">Cuántas rutinas cambiaron y qué vehículos fallaron. Un vehículo fallido no revierte a los demás.</response>
    /// <response code="400">Falta la fecha, no tiene el formato YYYY-MM-DD, o el JSON no es válido.</response>
    [HttpPost("/api/processes/daily-evaluation")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public ActionResult<EvaluationResponse> DailyEvaluation(EvaluationRequest? request)
    {
        if (RequestValidator.Evaluation(request) is string error)
            return InvalidRequest.For(error);

        EvaluationResult result = mediator.Send(new EvaluateRoutinesByTime(request!.Date!.Value));
        foreach (Guid vehicleId in result.FailedVehicles)
            logger.LogError("The daily evaluation could not complete vehicle {VehicleId}.", vehicleId);

        return Ok(new EvaluationResponse(result.ChangedRoutines, result.FailedVehicles));
    }
}
