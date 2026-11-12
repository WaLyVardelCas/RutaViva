namespace RutaViva.Application;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    TResult Execute(TCommand command);
}
