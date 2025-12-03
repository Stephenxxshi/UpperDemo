using Microsoft.Extensions.Hosting;
using NLog;
using Plant01.Upper.Application.Models.Logging;

namespace Plant01.Upper.Presentation.Bootstrapper;

public class NLogConfigurationService : IHostedService
{
    private readonly ILogStore _logStore;

    public NLogConfigurationService(ILogStore logStore)
    {
        _logStore = logStore;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure we have a configuration
        var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();
        
        // Create and register the target
        var target = new NLogToLogStoreTarget(_logStore);
        config.AddTarget(target);
        
        // Add a rule to forward all logs to this target
        // You might want to adjust minLevel based on config, but here we capture everything 
        // and let the UI filter it, or let NLog config handle it if loaded from file.
        // If NLog.config exists, this appends to it.
        config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, target);
        
        // Apply changes
        LogManager.Configuration = config;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
