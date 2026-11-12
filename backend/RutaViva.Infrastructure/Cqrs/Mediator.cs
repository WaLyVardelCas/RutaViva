using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using RutaViva.Application;

namespace RutaViva.Infrastructure;

public sealed class Mediator(IServiceProvider provider) : IMediator
{
    public TResult Send<TResult>(ICommand<TResult> command) =>
        Invoke<TResult>(typeof(ICommandHandler<,>), command);

    public TResult Query<TResult>(IQuery<TResult> query) =>
        Invoke<TResult>(typeof(IQueryHandler<,>), query);

    private TResult Invoke<TResult>(Type definition, object message)
    {
        Type type = definition.MakeGenericType(message.GetType(), typeof(TResult));
        object handler = provider.GetRequiredService(type);
        MethodInfo method = type.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.Execute))
            ?? throw new InvalidOperationException($"Handler {type.Name} does not expose Execute.");

        try
        {
            return (TResult)method.Invoke(handler, [message])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}
