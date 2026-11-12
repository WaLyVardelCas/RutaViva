using RutaViva.Domain;

namespace RutaViva.Domain.Tests;

public class ServiceOrderTests
{
    private static readonly CycleReference Reference = new(DomainConstants.FirstCycle, 40_000, new DateOnly(2026, 1, 1));
    private static readonly DateOnly Today = new(2026, 7, 5);

    public static IEnumerable<object[]> AmountsUpToTheLimit()
    {
        yield return [300_000m];
        yield return [DomainConstants.ClientApprovalThreshold];
    }

    [Theory]
    [MemberData(nameof(AmountsUpToTheLimit))]
    public void Amount_up_to_the_limit_is_approved_automatically(decimal amount)
    {
        var order = CreateOrder();
        order.SubmitQuote(amount, Today);

        Assert.Equal(OrderStatus.Approved, order.Status);
        Assert.True(order.AutomaticApproval);
        Assert.NotEqual(OrderStatus.Quoted, order.Status);
        var domainEvents = order.ConsumeEvents();
        Assert.Contains(domainEvents, e => e is OrderQuoted);
        Assert.Contains(domainEvents, e => e is QuoteApproved { Automatic: true });
    }

    [Fact]
    public void Amount_above_the_limit_stays_quoted()
    {
        var order = CreateOrder();
        order.SubmitQuote(DomainConstants.ClientApprovalThreshold + 1, Today);
        Assert.Equal(OrderStatus.Quoted, order.Status);
        Assert.Null(order.AutomaticApproval);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejects_a_non_positive_amount(decimal amount)
    {
        var order = CreateOrder();
        Assert.Throws<BusinessRuleException>(() => order.SubmitQuote(amount, Today));
        Assert.Equal(OrderStatus.Created, order.Status);
    }

    [Fact]
    public void Does_not_allow_a_second_quote()
    {
        var order = CreateOrder();
        order.SubmitQuote(800_000, Today);
        Assert.Throws<BusinessRuleException>(() => order.SubmitQuote(900_000, Today));
        Assert.Equal(800_000, order.Quote!.Amount.Amount);
    }

    [Fact]
    public void The_client_approves_only_a_quoted_order()
    {
        var order = OrderQuoted();
        order.Approve();
        Assert.Equal(OrderStatus.Approved, order.Status);
        Assert.False(order.AutomaticApproval);

        var automatic = CreateOrder();
        automatic.SubmitQuote(100_000, Today);
        Assert.Throws<BusinessRuleException>(() => automatic.Approve());
    }

    [Fact]
    public void Reject_cancels_only_a_quoted_order()
    {
        var order = OrderQuoted();
        order.Reject();
        Assert.Equal(OrderStatus.Canceled, order.Status);

        var createdItem = CreateOrder();
        Assert.Throws<BusinessRuleException>(() => createdItem.Reject());
        Assert.Equal(OrderStatus.Created, createdItem.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Created)]
    [InlineData(OrderStatus.Quoted)]
    [InlineData(OrderStatus.Approved)]
    public void The_client_cancels_before_execution(OrderStatus status)
    {
        var order = WithStatus(status);
        order.Cancel();
        Assert.Equal(OrderStatus.Canceled, order.Status);
    }

    [Fact]
    public void Does_not_cancel_in_progress_or_finished_orders()
    {
        var inProgress = WithStatus(OrderStatus.InProgress);
        Assert.Throws<BusinessRuleException>(() => inProgress.Cancel());
        Assert.Equal(OrderStatus.InProgress, inProgress.Status);

        var completedOrder = WithStatus(OrderStatus.Completed);
        Assert.Throws<BusinessRuleException>(() => completedOrder.Cancel());
    }

    [Fact]
    public void Starts_only_an_approved_order()
    {
        var order = WithStatus(OrderStatus.Approved);
        order.Start();
        Assert.Equal(OrderStatus.InProgress, order.Status);

        var quoted = OrderQuoted();
        Assert.Throws<BusinessRuleException>(() => quoted.Start());
    }

    [Fact]
    public void Complete_records_the_execution()
    {
        var order = WithStatus(OrderStatus.InProgress);
        order.Complete(50_000, Today);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(50_000, order.Execution!.ActualMileage);
        Assert.Single(order.ConsumeEvents().OfType<ServiceOrderCompleted>());
    }

    [Fact]
    public void Complete_outside_the_state_or_with_invalid_data_does_not_change_the_order()
    {
        var approved = WithStatus(OrderStatus.Approved);
        Assert.Throws<BusinessRuleException>(() => approved.Complete(50_000, Today));
        Assert.Equal(OrderStatus.Approved, approved.Status);

        var inProgress = WithStatus(OrderStatus.InProgress);
        Assert.Throws<BusinessRuleException>(() => inProgress.Complete(39_000, Today));
        Assert.Throws<BusinessRuleException>(() => inProgress.Complete(40_000, new DateOnly(2025, 12, 31)));
        Assert.Equal(OrderStatus.InProgress, inProgress.Status);
        Assert.Null(inProgress.Execution);
    }

    private static ServiceOrder CreateOrder() => ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Reference);

    private static ServiceOrder OrderQuoted()
    {
        var order = CreateOrder();
        order.SubmitQuote(2_000_000, Today);
        return order;
    }

    private static ServiceOrder WithStatus(OrderStatus status)
    {
        var order = CreateOrder();
        if (status == OrderStatus.Created)
            return order;

        order.SubmitQuote(status == OrderStatus.Quoted ? 2_000_000 : 100_000, Today);
        if (status == OrderStatus.Quoted)
            return order;

        if (status == OrderStatus.Approved)
            return order;

        order.Start();
        if (status == OrderStatus.InProgress)
            return order;

        order.Complete(40_000, Today);
        return order;
    }
}
