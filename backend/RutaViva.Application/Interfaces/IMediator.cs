namespace RutaViva.Application;

public interface IMediator
{
    TResult Send<TResult>(ICommand<TResult> command);

    TResult Query<TResult>(IQuery<TResult> query);
}
