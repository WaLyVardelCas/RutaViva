namespace RutaViva.Application;

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    TResult Execute(TQuery query);
}
