namespace RutaViva.Application;

public static class Retry
{
    public static void Execute(Action action)
    {
        for (int attempt = ApplicationConstants.FirstRetryAttempt; ; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch (ConcurrencyConflictException) when (attempt < ApplicationConstants.MaximumRetryAttempts)
            {
            }
        }
    }
}
