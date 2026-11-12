using Microsoft.Extensions.Logging.Abstractions;
using RutaViva.Application;
using RutaViva.Domain;
using RutaViva.Infrastructure;

namespace RutaViva.Application.Tests;

internal sealed class AppFixture
{
    public InMemoryVehicleRepository Vehicles { get; } = new();
    public InMemoryOrderRepository Orders { get; } = new();
    public EventLog EventLog { get; } = new(NullLogger<EventLog>.Instance);
    public RegisterVehicleHandler Register { get; }
    public AddRoutineHandler Routines { get; }
    public RegisterMileageReadingHandler Readings { get; }
    public EvaluateRoutinesByTimeHandler Evaluation { get; }
    public SubmitQuoteHandler SubmitQuote { get; }
    public RejectQuoteHandler Reject { get; }
    public StartServiceHandler Start { get; }
    public CompleteServiceHandler Complete { get; }
    public RestartCycleWhenOrderCompleted Restart { get; }

    public AppFixture()
    {
        CreateOrderWhenRoutineOverdue createOrderHandler = new CreateOrderWhenRoutineOverdue(Orders, EventLog);
        Restart = new RestartCycleWhenOrderCompleted(Vehicles, EventLog);
        DomainEventDispatcher dispatcher = new DomainEventDispatcher(EventLog, [createOrderHandler, Restart]);
        Register = new RegisterVehicleHandler(Vehicles);
        Routines = new AddRoutineHandler(Vehicles);
        Readings = new RegisterMileageReadingHandler(Vehicles, new InMemoryReadingRegistry(), dispatcher);
        Evaluation = new EvaluateRoutinesByTimeHandler(Vehicles, dispatcher);
        var clock = TimeProvider.System;
        SubmitQuote = new SubmitQuoteHandler(Orders, dispatcher, clock);
        Reject = new RejectQuoteHandler(Orders, dispatcher);
        Start = new StartServiceHandler(Orders, dispatcher);
        Complete = new CompleteServiceHandler(Orders, dispatcher);
    }
}
