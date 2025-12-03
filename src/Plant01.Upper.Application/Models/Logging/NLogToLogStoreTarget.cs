using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using Plant01.Upper.Application.Models.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Plant01.Upper.Application.Models.Logging;

[Target("LogStore")]
public class NLogToLogStoreTarget : TargetWithLayout
{
    private readonly ILogStore _logStore;

    public NLogToLogStoreTarget(ILogStore logStore)
    {
        _logStore = logStore;
        Name = "LogStore"; // Default name
        Layout = "${message}"; // Default to just message to avoid duplication of timestamp/level
    }

    protected override void Write(LogEventInfo logEvent)
    {
        var message = RenderLogEvent(Layout, logEvent);

        // Map NLog level to Microsoft.Extensions.Logging.LogLevel
        LogLevel level = LogLevel.None;
        
        if (logEvent.Level == NLog.LogLevel.Trace) level = LogLevel.Trace;
        else if (logEvent.Level == NLog.LogLevel.Debug) level = LogLevel.Debug;
        else if (logEvent.Level == NLog.LogLevel.Info) level = LogLevel.Information;
        else if (logEvent.Level == NLog.LogLevel.Warn) level = LogLevel.Warning;
        else if (logEvent.Level == NLog.LogLevel.Error) level = LogLevel.Error;
        else if (logEvent.Level == NLog.LogLevel.Fatal) level = LogLevel.Critical;

        int eventId = 0;
        if (logEvent.Properties.TryGetValue("EventId", out var eventIdObj) && eventIdObj is int id)
        {
            eventId = id;
        }
        // Microsoft.Extensions.Logging often puts EventId in properties differently, 
        // but NLog.Extensions.Logging handles this mapping usually. 
        // If we use NLog.Extensions.Logging, it might populate properties.

        var logModel = new LogModel
        {
            Timestamp = logEvent.TimeStamp,
            Level = level,
            Message = message,
            Exception = logEvent.Exception?.ToString(),
            Category = logEvent.LoggerName ?? "",
            EventId = eventId
        };

        _logStore.AddLog(logModel);
    }
}
