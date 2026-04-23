using Blocks.Domain.Time;
using Lending.Application.Features.Loans.Commands.AccrueDailyPenalties;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lending.Presentation.Jobs;

public class AccrueDailyPenaltiesJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccrueDailyPenaltiesJob> _logger;

    public AccrueDailyPenaltiesJob(IServiceScopeFactory scopeFactory, ILogger<AccrueDailyPenaltiesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
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
            
            _logger.LogInformation("AccrueDailyPenaltiesJob scheduled for {NextRun} (RD time).", nextRun);

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender.Send(new AccrueDailyPenaltiesCommand(), stoppingToken);

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("AccrueDailyPenaltiesJob executed successfully at {ExecutedAt} (RD time).",
                        DateProvider.Now());
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AccrueDailyPenaltiesJob was cancelled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing AccrueDailyPenaltiesJob.");
            }
        }
    }
}