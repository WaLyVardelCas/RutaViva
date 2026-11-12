using Microsoft.AspNetCore.Mvc;
using RutaViva.Application;
using RutaViva.Domain;

namespace RutaViva.Api;

/// <summary>Ciclo de la orden de servicio.</summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Órdenes")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>Lista órdenes. Se puede filtrar por estado y por vehículo.</summary>
    /// <param name="status">Created, Quoted, Approved, InProgress, Completed o Canceled. Vacío para no filtrar.</param>
    /// <param name="vehicleId">Identificador del vehículo. Vacío para no filtrar.</param>
    /// <response code="200">Órdenes que cumplen el filtro.</response>
    /// <response code="400">El estado indicado no existe.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<OrderDto>> List([FromQuery] string? status, [FromQuery] Guid? vehicleId)
    {
        if (RequestValidator.Status(status, out OrderStatus? filter) is string error)
            return InvalidRequest.For(error);
        return Ok(mediator.Query(new ListOrders(filter, vehicleId)));
    }

    /// <summary>Consulta una orden, incluida su cotización.</summary>
    /// <response code="200">Orden encontrada.</response>
    /// <response code="404">La orden no existe.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public ActionResult<OrderDto> Get(Guid id) =>
        Ok(mediator.Query(new GetOrder(id)));

    /// <summary>Registra la cotización. Hasta COP 1.500.000 la orden pasa a Approved; por encima queda Quoted.</summary>
    /// <response code="200">Orden con el monto y el estado resultante.</response>
    /// <response code="400">Falta el monto o el JSON no es válido.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">La orden no está Created, el monto no es mayor que cero o la cotización ya fue enviada.</response>
    [HttpPost("{id:guid}/quote")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Quote(Guid id, QuoteRequest? request)
    {
        if (RequestValidator.Quote(request) is string error)
            return InvalidRequest.For(error);
        return Ok(mediator.Send(new SubmitQuote(id, request!.Amount!.Value)));
    }

    /// <summary>El cliente aprueba una cotización que superó COP 1.500.000.</summary>
    /// <response code="200">Orden en Approved, con aprobación manual.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">La orden no está Quoted.</response>
    [HttpPost("{id:guid}/approval")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Approve(Guid id) =>
        Ok(mediator.Send(new ApproveQuote(id)));

    /// <summary>El cliente rechaza la cotización. La orden queda Canceled y la rutina sigue Overdue.</summary>
    /// <response code="200">Orden cancelada.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">La orden no está Quoted.</response>
    [HttpPost("{id:guid}/rejection")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Reject(Guid id) =>
        Ok(mediator.Send(new RejectQuote(id)));

    /// <summary>Cancela la orden antes de iniciar el servicio. No genera otra orden para la misma rutina.</summary>
    /// <response code="200">Orden cancelada.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">El servicio ya inició, o la orden ya está cerrada.</response>
    [HttpPost("{id:guid}/cancellation")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Cancel(Guid id) =>
        Ok(mediator.Send(new CancelOrder(id)));

    /// <summary>Inicia el servicio de una orden aprobada.</summary>
    /// <response code="200">Orden en InProgress.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">La orden no está Approved.</response>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Start(Guid id) =>
        Ok(mediator.Send(new StartService(id)));

    /// <summary>Completa el servicio e inicia el siguiente ciclo de la rutina.</summary>
    /// <response code="200">Orden en Completed. La rutina vuelve a UpToDate con la nueva referencia.</response>
    /// <response code="400">Faltan el kilometraje o la fecha reales, o el JSON no es válido.</response>
    /// <response code="404">La orden no existe.</response>
    /// <response code="409">La orden no está InProgress, o el kilometraje o la fecha son anteriores a la referencia del ciclo.</response>
    [HttpPost("{id:guid}/completion")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public ActionResult<OrderDto> Complete(Guid id, CompleteServiceRequest? request)
    {
        if (RequestValidator.Completion(request) is string error)
            return InvalidRequest.For(error);
        return Ok(mediator.Send(new CompleteService(id, request!.Mileage!.Value, request.Date!.Value)));
    }
}
