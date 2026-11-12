using Microsoft.Extensions.Logging.Abstractions;
using RutaViva.Application;
using RutaViva.Domain;
using RutaViva.Infrastructure;

namespace RutaViva.Application.Tests;

public class ApplicationFlowTests
{
    private readonly DateOnly _start = new(2026, 1, 1);

    [Fact]
    public void A_reading_that_expires_a_routine_creates_one_order()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("ABC123", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));

        fixture.Readings.Execute(new RegisterMileageReading("evt-vence", "ABC123", 30_000));
        fixture.Readings.Execute(new RegisterMileageReading("evt-vence", "ABC123", 31_000));

        Assert.Equal(2, fixture.Orders.List(null, vehicle.Id).Count);
        Assert.Equal(2, fixture.EventLog.List().OfType<RoutineOverdue>().Count());
    }

    [Fact]
    public void A_reading_does_not_evaluate_time_and_the_daily_job_does_not_evaluate_mileage()
    {
        AppFixture fixture = new AppFixture();
        var timeOnly = fixture.Register.Execute(new RegisterVehicle("TIE001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(timeOnly.Id, "Solo tiempo", null, 6, _start));
        fixture.Readings.Execute(new RegisterMileageReading("evt-t", "TIE001", 90_000));
        Assert.Equal(RoutineStatus.UpToDate, fixture.Vehicles.Get(timeOnly.Id)!.Routines.Single(routine => routine.Name == "Solo tiempo").Status);

        AppFixture mileageOnly = new AppFixture();
        var mileageVehicle = mileageOnly.Register.Execute(new RegisterVehicle("KM0001", 20_000, _start));
        mileageOnly.Routines.Execute(new AddRoutine(mileageVehicle.Id, "Solo km", 10_000, null, _start));
        var result = mileageOnly.Evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 12, 1)));
        Assert.Equal(2, result.ChangedRoutines);
        Assert.Equal(RoutineStatus.UpToDate, mileageOnly.Vehicles.Get(mileageVehicle.Id)!.Routines.Single(routine => routine.Name == "Solo km").Status);
    }

    [Fact]
    public void The_daily_job_is_idempotent_and_does_not_regress()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("DIA001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", null, 6, _start));

        var firstCopy = fixture.Evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 7, 1)));
        var secondCopy = fixture.Evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 7, 1)));
        var earlierRun = fixture.Evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 1, 2)));

        Assert.Equal(2, firstCopy.ChangedRoutines);
        Assert.Equal(0, secondCopy.ChangedRoutines);
        Assert.Equal(0, earlierRun.ChangedRoutines);
        var stored = fixture.Vehicles.Get(vehicle.Id)!;
        Assert.Equal(RoutineStatus.Overdue, stored.Routines.Single(routine => routine.Name == DomainConstants.OilChange).Status);
        Assert.Equal(RoutineStatus.Overdue, stored.Routines.Single(routine => routine.Name == "Aceite").Status);
        Assert.Equal(2, fixture.Orders.List(null, vehicle.Id).Count);
    }

    [Fact]
    public void One_vehicle_failure_does_not_stop_the_others()
    {
        InMemoryVehicleRepository store = new InMemoryVehicleRepository();
        Vehicle a = Vehicle.Register("A1", 0, _start);
        Vehicle b = Vehicle.Register("B1", 0, _start);
        Vehicle c = Vehicle.Register("C1", 0, _start);
        store.Save(a);
        store.Save(b);
        store.Save(c);
        a = store.GetByPlate(new Plate("A1"))!;
        b = store.GetByPlate(new Plate("B1"))!;
        c = store.GetByPlate(new Plate("C1"))!;
        a.AddRoutine("R", null, 6, _start);
        b.AddRoutine("R", null, 6, _start);
        c.AddRoutine("R", null, 6, _start);
        store.Save(a);
        store.Save(b);
        store.Save(c);

        RepositoryThatFailsOnSave failingStore = new RepositoryThatFailsOnSave(store, b.Id);
        InMemoryOrderRepository orders = new InMemoryOrderRepository();
        EventLog eventLog = new EventLog(NullLogger<EventLog>.Instance);
        DomainEventDispatcher dispatcher = new DomainEventDispatcher(eventLog, [new CreateOrderWhenRoutineOverdue(orders, eventLog)]);
        EvaluateRoutinesByTimeHandler evaluation = new EvaluateRoutinesByTimeHandler(failingStore, dispatcher);

        var result = evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 7, 1)));

        Assert.Equal([b.Id], result.FailedVehicles);
        Assert.Equal(RoutineStatus.Overdue, store.Get(a.Id)!.Routines[0].Status);
        Assert.Equal(RoutineStatus.UpToDate, store.Get(b.Id)!.Routines[0].Status);
        Assert.Equal(RoutineStatus.Overdue, store.Get(c.Id)!.Routines[0].Status);
    }

    [Fact]
    public async Task Concurrent_readings_keep_the_highest_mileage_and_one_order()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("PAR001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));

        await Task.WhenAll(
            Task.Run(() => fixture.Readings.Execute(new RegisterMileageReading("c1", "PAR001", 30_000))),
            Task.Run(() => fixture.Readings.Execute(new RegisterMileageReading("c2", "PAR001", 30_100))));

        Assert.Equal(30_100, fixture.Vehicles.Get(vehicle.Id)!.CurrentMileage.Value);
        var orders = fixture.Orders.List(null, vehicle.Id);
        Assert.Equal(2, orders.Count);
        Assert.Equal(2, orders.Select(order => order.RoutineId).Distinct().Count());
    }

    [Fact]
    public async Task The_same_concurrent_event_is_applied_once()
    {
        AppFixture fixture = new AppFixture();
        fixture.Register.Execute(new RegisterVehicle("SIM001", 25_000, _start));

        await Task.WhenAll(
            Task.Run(() => fixture.Readings.Execute(new RegisterMileageReading("evt-3", "SIM001", 25_500))),
            Task.Run(() => fixture.Readings.Execute(new RegisterMileageReading("evt-3", "SIM001", 25_500))));

        Assert.Equal(25_500, fixture.Vehicles.GetByPlate(new Plate("SIM001"))!.CurrentMileage.Value);
    }

    [Fact]
    public void Replaying_an_event_does_not_apply_a_different_body()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("REP001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));

        Assert.Equal(ReadingResult.Accepted, fixture.Readings.Execute(new RegisterMileageReading("evt-1", "REP001", 29_000)));
        Assert.Equal(ReadingResult.Accepted, fixture.Readings.Execute(new RegisterMileageReading("evt-1", "REP001", 31_000)));

        var stored = fixture.Vehicles.Get(vehicle.Id)!;
        Assert.Equal(29_000, stored.CurrentMileage.Value);
        Assert.Equal(RoutineStatus.DueSoon, stored.Routines.Single(routine => routine.Name == "Aceite").Status);
        Assert.Equal(RoutineStatus.DueSoon, stored.Routines.Single(routine => routine.Name == DomainConstants.OilChange).Status);
        Assert.Equal(2, fixture.EventLog.List().OfType<RoutineDueSoon>().Count());
    }

    [Fact]
    public void A_stale_copy_does_not_overwrite_the_stored_aggregate()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("VER001", 25_000, _start));
        var firstCopy = fixture.Vehicles.Get(vehicle.Id)!;
        var secondCopy = fixture.Vehicles.Get(vehicle.Id)!;

        Assert.True(firstCopy.RegisterReading(26_000));
        fixture.Vehicles.Save(firstCopy);
        Assert.True(secondCopy.RegisterReading(27_000));

        Assert.Throws<ConcurrencyConflictException>(() => fixture.Vehicles.Save(secondCopy));
        Assert.Equal(26_000, fixture.Vehicles.Get(vehicle.Id)!.CurrentMileage.Value);
    }

    [Fact]
    public void Completing_restarts_the_cycle_without_changing_vehicle_mileage()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("FIN001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));
        fixture.Readings.Execute(new RegisterMileageReading("evt-fin", "FIN001", 52_000));

        var addedId = fixture.Vehicles.Get(vehicle.Id)!.Routines.Single(routine => routine.Name == "Aceite").Id;
        var order = Assert.Single(fixture.Orders.List(null, vehicle.Id), item => item.RoutineId == addedId);
        fixture.SubmitQuote.Execute(new SubmitQuote(order.Id, 900_000));
        fixture.Start.Execute(new StartService(order.Id));
        var completedOrder = fixture.Complete.Execute(new CompleteService(order.Id, 50_000, new DateOnly(2026, 7, 5)));

        Assert.Equal(OrderStatus.Completed, completedOrder.Status);
        var stored = fixture.Vehicles.Get(vehicle.Id)!;
        var aceite = stored.Routines.Single(routine => routine.Name == "Aceite");
        Assert.Equal(52_000, stored.CurrentMileage.Value);
        Assert.Equal(RoutineStatus.UpToDate, aceite.Status);
        Assert.Equal(DomainConstants.FirstCycle + DomainConstants.CycleStep, aceite.Cycle.Number);
        Assert.Equal(50_000, aceite.Cycle.ReferenceMileage);

        var domainEvent = Assert.Single(fixture.EventLog.List().OfType<ServiceOrderCompleted>());
        fixture.Restart.Handle(domainEvent);
        Assert.Equal(DomainConstants.FirstCycle + DomainConstants.CycleStep, fixture.Vehicles.Get(vehicle.Id)!.Routines.Single(routine => routine.Name == "Aceite").Cycle.Number);
    }

    [Fact]
    public void Canceling_or_rejecting_does_not_create_another_order()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("CAN001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));
        fixture.Readings.Execute(new RegisterMileageReading("evt-can", "CAN001", 30_000));

        var addedId = fixture.Vehicles.Get(vehicle.Id)!.Routines.Single(routine => routine.Name == "Aceite").Id;
        var order = Assert.Single(fixture.Orders.List(null, vehicle.Id), item => item.RoutineId == addedId);
        fixture.SubmitQuote.Execute(new SubmitQuote(order.Id, 2_000_000));
        var rejectedOrder = fixture.Reject.Execute(new RejectQuote(order.Id));

        Assert.Equal(OrderStatus.Canceled, rejectedOrder.Status);
        Assert.Equal(RoutineStatus.Overdue, fixture.Vehicles.Get(vehicle.Id)!.Routines.Single(routine => routine.Name == "Aceite").Status);

        fixture.Readings.Execute(new RegisterMileageReading("evt-can-2", "CAN001", 31_000));
        fixture.Evaluation.Execute(new EvaluateRoutinesByTime(new DateOnly(2026, 8, 1)));
        Assert.Equal(2, fixture.Orders.List(null, vehicle.Id).Count);
    }

    [Fact]
    public void A_higher_actual_mileage_does_not_update_the_vehicle()
    {
        AppFixture fixture = new AppFixture();
        var vehicle = fixture.Register.Execute(new RegisterVehicle("MAY001", 20_000, _start));
        fixture.Routines.Execute(new AddRoutine(vehicle.Id, "Aceite", 10_000, null, _start));
        fixture.Readings.Execute(new RegisterMileageReading("evt-may", "MAY001", 50_000));
        var addedId = fixture.Vehicles.Get(vehicle.Id)!.Routines.Single(routine => routine.Name == "Aceite").Id;
        var order = Assert.Single(fixture.Orders.List(null, vehicle.Id), item => item.RoutineId == addedId);
        fixture.SubmitQuote.Execute(new SubmitQuote(order.Id, 100_000));
        fixture.Start.Execute(new StartService(order.Id));
        fixture.Complete.Execute(new CompleteService(order.Id, 50_300, new DateOnly(2026, 7, 5)));

        var stored = fixture.Vehicles.Get(vehicle.Id)!;
        Assert.Equal(50_000, stored.CurrentMileage.Value);
        Assert.Equal(50_300, stored.Routines.Single(routine => routine.Name == "Aceite").Cycle.ReferenceMileage);
    }
}

internal sealed class RepositoryThatFailsOnSave(InMemoryVehicleRepository inner, Guid failingId) : IVehicleRepository
{
    public Vehicle? Get(Guid id) => inner.Get(id);
    public Vehicle? GetByPlate(Plate plate) => inner.GetByPlate(plate);
    public IReadOnlyList<Vehicle> List() => inner.List();

    public void Save(Vehicle vehicle)
    {
        if (vehicle.Id == failingId)
            throw new InvalidOperationException("falla simulada");
        inner.Save(vehicle);
    }
}
