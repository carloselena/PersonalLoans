using Blocks.Domain.Time;
using Lending.Application.Features.Loans.Commands.AccrueDailyPenalties;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lending.Presentation.Jobs;

public class AccrueDailyPenaltiesJob(IServiceScopeFactory scopeFactory, ILogger<AccrueDailyPenaltiesJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateProvider.Now();
            
            var nextRun = new DateTimeOffset(
                now.Year,
                now.Month,
                now.Day,
                0, 5, 0,
                now.Offset);
                
            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            
            logger.LogInformation("AccrueDailyPenaltiesJob scheduled for {NextRun} (RD time).", nextRun);

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();

                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender.Send(new AccrueDailyPenaltiesCommand(), stoppingToken);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("AccrueDailyPenaltiesJob executed successfully at {ExecutedAt} (RD time).",
                        DateProvider.Now());
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("AccrueDailyPenaltiesJob was cancelled.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing AccrueDailyPenaltiesJob.");
            }
        }
    }
}