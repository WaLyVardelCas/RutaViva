using Microsoft.AspNetCore.Mvc;
using RutaViva.Application;

namespace RutaViva.Api;

/// <summary>Configuración y consulta de rutinas.</summary>
[ApiController]
[Route("api/vehicles/{vehicleId:guid}/routines")]
[Produces("application/json")]
[Tags("Rutinas")]
public sealed class RoutinesController(IMediator mediator, TimeProvider clock) : ControllerBase
{
    /// <summary>Agrega una rutina al vehículo. El intervalo queda fijo: no hay operación para editarlo.</summary>
    /// <response code="200">Rutina creada, con el primer ciclo tomado del kilometraje y la fecha de este momento.</response>
    /// <response code="400">Falta el nombre o el JSON no es válido.</response>
    /// <response code="404">El vehículo no existe.</response>
    /// <response code="409">Nombre repetido, sin intervalo, o intervalo menor o igual a cero.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<RoutineDto> Add(Guid vehicleId, RoutineRequest? request)
    {
        if (RequestValidator.Routine(request) is string error)
            return InvalidRequest.For(error);
        DateOnly date = request!.Date ?? Clock.Today(clock);
        return Ok(mediator.Send(new AddRoutine(vehicleId, request.Name!, request.Kilometers, request.Months, date)));
    }

    /// <summary>Lista las rutinas del vehículo, incluidas las de base.</summary>
    /// <response code="200">Rutinas del vehículo.</response>
    /// <response code="404">El vehículo no existe.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoutineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<RoutineDto>> List(Guid vehicleId) =>
        Ok(mediator.Query(new ListRoutines(vehicleId)));
}
