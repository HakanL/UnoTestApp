using System;
using System.Collections.Generic;

namespace DMXCore.DMXCore100;

public class StartupService : IHostedService
{
    private readonly Microsoft.Extensions.Logging.ILogger log;
    private readonly IServiceProvider serviceProvider;

    public StartupService(ILogger<StartupService> logger, IServiceProvider serviceProvider)
    {
        this.log = logger;
        this.serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var mainApp = this.serviceProvider.GetRequiredService<ILifetimeControl>();
            mainApp.RegisterShutdownEvent(() =>
            {
                this.log.LogWarning("Shutdown token triggered");

                mainApp.SetSystemShuttingDown();
                mainApp.SetSystemShutdownEvent();
                mainApp.Exit();
            });

            this.log.LogInformation("Current culture {CurrentCulture}", CultureInfo.DefaultThreadCurrentCulture);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                var ex = e.Exception.InnerException;

                if (ex is System.Net.WebSockets.WebSocketException)
                    // Ignore
                    return;

                if (ex is System.Net.Sockets.SocketException)
                    // Ignore
                    return;

                if (ex is System.Threading.Tasks.TaskCanceledException)
                    // Ignore
                    return;

                this.log.LogError(ex, "Unhandled Task exception: {Message}", ex.Message);
            };
        }
        catch (Exception ex)
        {
            this.log.LogCritical(ex, "Failed to start: {Message}", ex.Message);

            Console.WriteLine($"Failed to start: {ex}");

#if DEBUG
            throw;
#endif
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
