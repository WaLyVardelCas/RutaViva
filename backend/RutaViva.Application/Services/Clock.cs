namespace RutaViva.Application;

public static class Clock
{
    private static readonly TimeZoneInfo Bogota = TimeZoneInfo.FindSystemTimeZoneById(ApplicationConstants.BogotaTimeZoneId);

    public static DateOnly Today(TimeProvider clock)
    {
        DateTimeOffset local = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Bogota);
        return DateOnly.FromDateTime(local.DateTime);
    }
}
