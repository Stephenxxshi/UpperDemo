using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Infrastructure.Services;

public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IDeviceCommunicationService _deviceService;

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IDeviceCommunicationService deviceService)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _deviceService = deviceService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Monitor Service Started (Bridge Mode).");
        
        // Subscribe to tag changes
        _deviceService.TagChanged += OnTagChanged;
        
        return Task.CompletedTask;
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        // Filter only relevant tags if needed
        // For now, we assume any tag change is a trigger or data update
        
        // Example mapping logic:
        // If tag name starts with "ST", it might be a station trigger
        // e.g. "ST01_Loading.Trigger"
        
        try
        {
            // Simple logic: Just forward everything for now, or filter by specific tags
            // In a real app, you'd look up the tag in a "TriggerMap"
            
            // Example: If value is boolean TRUE, trigger an event
            if (e.NewData.Value is bool bVal && bVal)
            {
                // Extract StationId from TagName (e.g. "SDJ01.HeartBreak" -> "SDJ01")
                var parts = e.TagName.Split('.');
                var stationId = parts.Length > 0 ? parts[0] : "Unknown";
                
                await _dispatcher.EnqueueAsync(
                    stationId: stationId,
                    source: TriggerSourceType.PLC,
                    payload: $"{e.TagName}={e.NewData.Value}",
                    priority: TriggerPriority.Normal,
                    debounceKey: e.TagName // Debounce by tag name
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tag change for {Tag}", e.TagName);
        }
    }

    public override void Dispose()
    {
        _deviceService.TagChanged -= OnTagChanged;
        base.Dispose();
    }
}
