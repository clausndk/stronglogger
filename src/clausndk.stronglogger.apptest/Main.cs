using Microsoft.Extensions.Hosting;

namespace clausndk.stronglogger.apptest;

public class Main(IStrongLogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.Log(LogLevel.Information, $"Hello world - {Guid.NewGuid():D}");

            if (i == 3)
            {
                logger.Log(LogLevel.Error, new AccessViolationException("401"), $"World fail - {Guid.NewGuid():D}");
                i = 0;
            }

            i++;
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}