using Microsoft.Extensions.Hosting;

namespace clausndk.stronglogger.apptest;

public class Main(IStrongLogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.Log(LogLevel.Verbose, $"Hello Verbose - {Guid.NewGuid():D}");
            logger.Log(LogLevel.Debug, $"Hello Debug - {Guid.NewGuid():D}");
            logger.Log(LogLevel.Information, $"Hello Information - {Guid.NewGuid():D}");
            logger.Log(LogLevel.Warning, $"Hello Warning - {Guid.NewGuid():D}");
            logger.Log(LogLevel.Error, $"Hello Error - {Guid.NewGuid():D}");

            try
            {
                throw new AccessViolationException("Du har ikke adgang", new ArgumentOutOfRangeException(nameof(stoppingToken), new DataMisalignedException("Har du styr på dine parametre?")));
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Fatal, e, $"World fail - {Guid.NewGuid():D}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}