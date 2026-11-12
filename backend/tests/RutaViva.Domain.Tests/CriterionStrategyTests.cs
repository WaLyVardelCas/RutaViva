using RutaViva.Domain;

namespace RutaViva.Domain.Tests;

public class CriterionStrategyTests
{
    private static readonly MaintenanceCycle Cycle = new(DomainConstants.FirstCycle, 20_000, new DateOnly(2026, 1, 1));
    private static readonly MaintenanceInterval TenThousandKilometers = new(10_000, null);
    private static readonly MaintenanceInterval SixMonths = new(null, 6);

    [Theory]
    [InlineData(28999, RoutineStatus.UpToDate)]
    [InlineData(29000, RoutineStatus.DueSoon)]
    [InlineData(30000, RoutineStatus.Overdue)]
    [InlineData(31000, RoutineStatus.Overdue)]
    public void Mileage_respects_the_threshold_and_the_limit(int current, RoutineStatus expected)
    {
        MileageSignal signal = new MileageSignal(current);
        Assert.True(MileageCriterion.Instance.Matches(TenThousandKilometers, signal));
        Assert.Equal(expected, MileageCriterion.Instance.Evaluate(Cycle, TenThousandKilometers, signal));
    }

    [Fact]
    public void Time_outside_the_threshold_stays_up_to_date()
    {
        TimeSignal signal = new TimeSignal(new DateOnly(2026, 6, 12));
        Assert.Equal(RoutineStatus.UpToDate, TimeCriterion.Instance.Evaluate(Cycle, SixMonths, signal));
    }

    [Fact]
    public void Time_inside_the_threshold_becomes_due_soon()
    {
        TimeSignal signal = new TimeSignal(new DateOnly(2026, 6, 13));
        Assert.Equal(RoutineStatus.DueSoon, TimeCriterion.Instance.Evaluate(Cycle, SixMonths, signal));
    }

    [Fact]
    public void Time_at_the_limit_becomes_overdue()
    {
        TimeSignal signal = new TimeSignal(new DateOnly(2026, 7, 1));
        Assert.Equal(RoutineStatus.Overdue, TimeCriterion.Instance.Evaluate(Cycle, SixMonths, signal));
    }

    [Fact]
    public void One_month_from_the_end_of_January_lands_on_the_last_day_of_February() =>
        Assert.Equal(new DateOnly(2026, 2, 28), TimeCriterion.Limit(new DateOnly(2026, 1, 31), 1));

    [Fact]
    public void Each_strategy_ignores_the_other_signal()
    {
        Assert.False(MileageCriterion.Instance.Matches(TenThousandKilometers, new TimeSignal(new DateOnly(2026, 7, 1))));
        Assert.False(TimeCriterion.Instance.Matches(SixMonths, new MileageSignal(30_000)));
        Assert.False(MileageCriterion.Instance.Matches(SixMonths, new MileageSignal(30_000)));
        Assert.False(TimeCriterion.Instance.Matches(TenThousandKilometers, new TimeSignal(new DateOnly(2026, 7, 1))));
    }
}
