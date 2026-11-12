using RutaViva.Domain;

namespace RutaViva.Domain.Tests;

public class VehicleRoutineTests
{
    private readonly DateOnly _today = new(2026, 1, 1);

    [Fact]
    public void Registers_a_vehicle_with_its_initial_mileage()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 20_000, _today);
        Assert.Equal(20_000, vehicle.CurrentMileage.Value);
        Assert.Equal("ABC123", vehicle.Plate.Value);
        Assert.Equal(BaseRoutines.All.Count, vehicle.Routines.Count);
        Assert.All(vehicle.Routines, routine =>
        {
            Assert.Equal(RoutineStatus.UpToDate, routine.Status);
            Assert.Equal(20_000, routine.Cycle.ReferenceMileage);
            Assert.Equal(_today, routine.Cycle.ReferenceDate);
        });
    }

    [Fact]
    public void Rejects_negative_initial_mileage() =>
        Assert.Throws<BusinessRuleException>(() => Vehicle.Register("ABC123", -1, _today));

    [Fact]
    public void Normalizes_the_plate()
    {
        Vehicle vehicle = Vehicle.Register(" abc 123 ", 0, _today);
        Assert.Equal("ABC123", vehicle.Plate.Value);
    }

    [Fact]
    public void Adds_a_routine_with_both_criteria_up_to_date()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 20_000, _today);
        var routine = vehicle.Routines.Single(item => item.Name == DomainConstants.OilChange);

        Assert.Equal(RoutineStatus.UpToDate, routine.Status);
        Assert.Equal(20_000, routine.Cycle.ReferenceMileage);
        Assert.Equal(_today, routine.Cycle.ReferenceDate);
        Assert.Equal(30_000, routine.MileageLimit);
        Assert.Equal(new DateOnly(2026, 7, 1), routine.DateLimit);
    }

    [Fact]
    public void Accepts_a_name_that_is_not_in_the_examples()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 0, _today);
        var routine = vehicle.AddRoutine("Revisión de suspensión", 20_000, null, _today);
        Assert.Equal("Revisión de suspensión", routine.Name);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0, null)]
    public void Rejects_an_invalid_interval(int? km, int? months)
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 0, _today);
        Assert.Throws<BusinessRuleException>(() => vehicle.AddRoutine("Aceite", km, months, _today));
    }

    [Fact]
    public void Rejects_a_duplicate_name_ignoring_case()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 0, _today);
        Assert.Throws<BusinessRuleException>(() => vehicle.AddRoutine(DomainConstants.OilChange.ToLowerInvariant(), 5_000, null, _today));
        vehicle.AddRoutine("Revisión de suspensión", 5_000, null, _today);
        Assert.Throws<BusinessRuleException>(() => vehicle.AddRoutine("revisión de suspensión", 8_000, null, _today));
    }

    [Fact]
    public void A_reading_at_the_threshold_becomes_due_soon_once()
    {
        var vehicle = VehicleWith(10_000, null);
        Assert.True(vehicle.RegisterReading(29_000));

        var routine = vehicle.Routines[0];
        Assert.Equal(RoutineStatus.DueSoon, routine.Status);
        Assert.Single(vehicle.ConsumeEvents().OfType<RoutineDueSoon>());

        vehicle.RegisterReading(29_100);
        Assert.Empty(vehicle.ConsumeEvents().OfType<RoutineDueSoon>());
    }

    [Fact]
    public void A_direct_jump_to_overdue_does_not_emit_due_soon()
    {
        var vehicle = VehicleWith(10_000, null);
        vehicle.RegisterReading(31_000);

        var domainEvents = vehicle.ConsumeEvents();
        Assert.Equal(RoutineStatus.Overdue, vehicle.Routines[0].Status);
        Assert.Empty(domainEvents.OfType<RoutineDueSoon>());
        Assert.Single(domainEvents.OfType<RoutineOverdue>());
    }

    [Fact]
    public void The_more_advanced_criterion_wins()
    {
        var dueSoonReading = VehicleWith(10_000, 6);
        dueSoonReading.RegisterReading(29_000);
        Assert.Equal(RoutineStatus.DueSoon, dueSoonReading.Routines[0].Status);

        var overdue = VehicleWith(10_000, 6);
        overdue.EvaluateByTime(new DateOnly(2026, 7, 1));
        Assert.Equal(RoutineStatus.Overdue, overdue.Routines[0].Status);
    }

    [Fact]
    public void A_favorable_evaluation_does_not_regress_the_status()
    {
        var vehicle = VehicleWith(10_000, 6);
        vehicle.RegisterReading(29_000);
        vehicle.ConsumeEvents();

        vehicle.EvaluateByTime(_today);
        Assert.Equal(RoutineStatus.DueSoon, vehicle.Routines[0].Status);
        Assert.Empty(vehicle.ConsumeEvents());
    }

    [Fact]
    public void The_second_criterion_does_not_repeat_the_notification()
    {
        var vehicle = VehicleWith(10_000, 6);
        vehicle.RegisterReading(29_000);
        vehicle.ConsumeEvents();

        vehicle.EvaluateByTime(new DateOnly(2026, 6, 13));
        Assert.Equal(RoutineStatus.DueSoon, vehicle.Routines[0].Status);
        Assert.Empty(vehicle.ConsumeEvents());
    }

    [Fact]
    public void Overdue_does_not_record_the_event_again()
    {
        var vehicle = VehicleWith(10_000, null);
        vehicle.RegisterReading(30_000);
        vehicle.ConsumeEvents();

        vehicle.RegisterReading(31_000);
        Assert.Empty(vehicle.ConsumeEvents());
    }

    [Fact]
    public void Ignores_lower_or_equal_readings()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 25_000, _today);
        Assert.False(vehicle.RegisterReading(25_000));
        Assert.False(vehicle.RegisterReading(24_000));
        Assert.Equal(25_000, vehicle.CurrentMileage.Value);
        Assert.Empty(vehicle.ConsumeEvents());
    }

    [Fact]
    public void Accepts_a_higher_reading_even_when_the_date_is_earlier()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 26_000, _today);
        Assert.True(vehicle.RegisterReading(26_200));
        Assert.Equal(26_200, vehicle.CurrentMileage.Value);
    }

    [Fact]
    public void A_reading_does_not_evaluate_a_time_only_routine()
    {
        Vehicle vehicle = Vehicle.Register("ABC123", 0, _today);
        var routine = vehicle.AddRoutine("Solo tiempo", null, 6, _today);
        vehicle.RegisterReading(100_000);
        Assert.Equal(RoutineStatus.UpToDate, routine.Status);
    }

    [Fact]
    public void Elapsed_time_does_not_change_until_the_daily_evaluation()
    {
        var vehicle = VehicleWith(10_000, 6);
        vehicle.RegisterReading(21_000);
        Assert.Equal(RoutineStatus.UpToDate, vehicle.Routines[0].Status);
    }

    [Fact]
    public void Restart_sets_the_routine_up_to_date_without_lowering_mileage()
    {
        var vehicle = VehicleWith(10_000, null);
        vehicle.RegisterReading(52_000);
        var routine = vehicle.Routines[0];
        Assert.True(vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 50_000, new DateOnly(2026, 7, 5)));

        Assert.Equal(RoutineStatus.UpToDate, routine.Status);
        Assert.Equal(DomainConstants.FirstCycle + DomainConstants.CycleStep, routine.Cycle.Number);
        Assert.Equal(50_000, routine.Cycle.ReferenceMileage);
        Assert.Equal(52_000, vehicle.CurrentMileage.Value);
    }

    [Fact]
    public void The_next_reading_notifies_the_new_cycle()
    {
        var vehicle = VehicleWith(10_000, null);
        vehicle.RegisterReading(52_000);
        var routine = vehicle.Routines[0];
        vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 50_000, new DateOnly(2026, 7, 5));
        vehicle.ConsumeEvents();

        vehicle.RegisterReading(59_100);
        Assert.Equal(RoutineStatus.DueSoon, routine.Status);
        Assert.Single(vehicle.ConsumeEvents().OfType<RoutineDueSoon>());
    }

    [Fact]
    public void Rejects_a_restart_earlier_than_the_reference()
    {
        var vehicle = VehicleWith(10_000, null);
        var routine = vehicle.Routines[0];
        Assert.Throws<BusinessRuleException>(() => vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 19_000, _today));
        Assert.Throws<BusinessRuleException>(() => vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 20_000, _today.AddDays(-1)));
    }

    [Fact]
    public void A_repeated_restart_has_no_effect()
    {
        var vehicle = VehicleWith(10_000, null);
        var routine = vehicle.Routines[0];
        Assert.True(vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 20_000, _today));
        Assert.False(vehicle.RestartCycle(routine.Id, DomainConstants.FirstCycle, 20_000, _today));
        Assert.Equal(DomainConstants.FirstCycle + DomainConstants.CycleStep, routine.Cycle.Number);
    }

    private Vehicle VehicleWith(int? km, int? months)
    {
        Vehicle vehicle = new Vehicle(Guid.NewGuid(), new Plate("ABC123"), new Mileage(20_000), DomainConstants.UnstoredVersion, []);
        vehicle.AddRoutine("Aceite", km, months, _today);
        return vehicle;
    }
}
