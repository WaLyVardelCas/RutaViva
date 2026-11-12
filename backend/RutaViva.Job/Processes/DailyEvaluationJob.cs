using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RutaViva.Application;

namespace RutaViva.Job;

public sealed class DailyEvaluationJob(
    IConfiguration configuration,
    IMediator mediator,
    TimeProvider clock,
    ILogger<DailyEvaluationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue(JobConstants.EvaluationEnabledKey, JobConstants.EvaluationEnabledByDefault))
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EvaluationResult result = mediator.Send(new EvaluateRoutinesByTime(Clock.Today(clock)));
                foreach (Guid vehicleId in result.FailedVehicles)
                    logger.LogError(JobConstants.VehicleFailedMessage, vehicleId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The daily evaluation failed.");
            }

            int intervalHours = configuration.GetValue(
                JobConstants.EvaluationIntervalHoursKey,
                JobConstants.DefaultEvaluationIntervalHours);
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }
}
