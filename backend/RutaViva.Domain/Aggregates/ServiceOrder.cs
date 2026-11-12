namespace RutaViva.Domain;

public sealed class ServiceOrder
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; }
    public Guid VehicleId { get; }
    public Guid RoutineId { get; }
    public CycleReference Reference { get; }
    public OrderStatus Status { get; private set; }
    public Quote? Quote { get; private set; }
    public bool? AutomaticApproval { get; private set; }
    public ServiceExecution? Execution { get; private set; }
    public int Version { get; private set; }

    public bool IsOpen => Status is not OrderStatus.Completed and not OrderStatus.Canceled;

    public ServiceOrder(
        Guid id,
        Guid vehicleId,
        Guid routineId,
        CycleReference reference,
        OrderStatus status,
        Quote? quote,
        bool? automaticApproval,
        ServiceExecution? execution,
        int version)
    {
        Id = id;
        VehicleId = vehicleId;
        RoutineId = routineId;
        Reference = reference;
        Status = status;
        Quote = quote;
        AutomaticApproval = automaticApproval;
        Execution = execution;
        Version = version;
    }

    public static ServiceOrder Create(Guid vehicleId, Guid routineId, CycleReference reference)
    {
        ServiceOrder order = new ServiceOrder(
            Guid.NewGuid(),
            vehicleId,
            routineId,
            reference,
            OrderStatus.Created,
            null,
            null,
            null,
            DomainConstants.UnstoredVersion);
        order._domainEvents.Add(new ServiceOrderCreated(order.Id, vehicleId, routineId));
        return order;
    }

    public void SubmitQuote(decimal amount, DateOnly date)
    {
        OrderValidator.RequireStatus(Status, OrderStatus.Created, "Solo se puede cotizar una orden creada.");
        OrderValidator.RequireNoExistingQuote(Quote is not null);

        Money money = new Money(amount);
        Quote = new Quote(money, date);
        _domainEvents.Add(new OrderQuoted(Id, money.Amount));

        if (money.RequiresClientApproval)
        {
            Status = OrderStatus.Quoted;
            return;
        }

        Status = OrderStatus.Approved;
        AutomaticApproval = true;
        _domainEvents.Add(new QuoteApproved(Id, Automatic: true));
    }

    public void Approve()
    {
        OrderValidator.RequireStatus(Status, OrderStatus.Quoted, "Solo el cliente puede aprobar una orden cotizada.");
        Status = OrderStatus.Approved;
        AutomaticApproval = false;
        _domainEvents.Add(new QuoteApproved(Id, Automatic: false));
    }

    public void Reject()
    {
        OrderValidator.RequireStatus(Status, OrderStatus.Quoted, "Solo se puede rechazar la cotización de una orden cotizada.");
        Status = OrderStatus.Canceled;
        _domainEvents.Add(new QuoteRejected(Id));
    }

    public void Cancel()
    {
        OrderValidator.RequireCancelable(Status);
        Status = OrderStatus.Canceled;
        _domainEvents.Add(new OrderCanceled(Id));
    }

    public void Start()
    {
        OrderValidator.RequireStatus(Status, OrderStatus.Approved, "Solo se puede iniciar una orden aprobada.");
        Status = OrderStatus.InProgress;
        _domainEvents.Add(new ServiceStarted(Id));
    }

    public void Complete(int actualMileage, DateOnly actualDate)
    {
        OrderValidator.RequireStatus(Status, OrderStatus.InProgress, "Solo se puede completar una orden en ejecución.");
        OrderValidator.RequireCompletion(
            actualMileage,
            Reference.ReferenceMileage,
            actualDate,
            Reference.ReferenceDate);

        Execution = new ServiceExecution(actualMileage, actualDate);
        Status = OrderStatus.Completed;
        _domainEvents.Add(new ServiceOrderCompleted(
            Id,
            VehicleId,
            RoutineId,
            Reference.CycleNumber,
            actualMileage,
            actualDate));
    }

    public IReadOnlyList<IDomainEvent> ConsumeEvents()
    {
        List<IDomainEvent> copia = _domainEvents.ToList();
        _domainEvents.Clear();
        return copia;
    }

    public void SetVersion(int version) => Version = version;
}
