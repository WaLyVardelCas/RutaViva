using Microsoft.AspNetCore.Mvc;
using RutaViva.Application;

namespace RutaViva.Api;

/// <summary>Registro y consulta de la flota.</summary>
[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
[Tags("Vehículos")]
public sealed class VehiclesController(IMediator mediator, TimeProvider clock) : ControllerBase
{
    /// <summary>Registra un vehículo con su kilometraje inicial.</summary>
    /// <response code="200">Vehículo registrado.</response>
    /// <response code="400">Faltan la placa o el kilometraje, o el JSON no es válido.</response>
    /// <response code="409">La placa ya existe o el kilometraje es negativo.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<VehicleDto> Register(VehicleRequest? request)
    {
        if (RequestValidator.Vehicle(request) is string error)
            return InvalidRequest.For(error);
        return Ok(mediator.Send(new RegisterVehicle(request!.Plate!, request.Mileage!.Value, Clock.Today(clock))));
    }

    /// <summary>Lista los vehículos registrados.</summary>
    /// <response code="200">Flota actual. Vacía si todavía no hay vehículos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<VehicleDto>> List() =>
        Ok(mediator.Query(new ListVehicles()));

    /// <summary>Consulta un vehículo.</summary>
    /// <response code="200">Vehículo encontrado.</response>
    /// <response code="404">No hay un vehículo con ese identificador.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public ActionResult<VehicleDto> Get(Guid id) =>
        Ok(mediator.Query(new GetVehicle(id)));
}
