using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Models.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Plant01.Upper.Application.Models.Logging;

public class SerilogToLogStoreSink : ILogEventSink
{
    private readonly ILogStore _logStore;
    private readonly IFormatProvider? _formatProvider;

    public SerilogToLogStoreSink(ILogStore logStore, IFormatProvider? formatProvider = null)
    {
        _logStore = logStore;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        
        // Map Serilog level to Microsoft.Extensions.Logging.LogLevel
        var level = logEvent.Level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.None
        };

        // Try to get Category from properties (SourceContext is standard in Serilog)
        string category = "";
        if (logEvent.Properties.TryGetValue("SourceContext", out var value))
        {
            category = value.ToString().Trim('"');
        }

        // Try to get EventId
        int eventId = 0;
        if (logEvent.Properties.TryGetValue("EventId", out var eventIdValue) && eventIdValue is StructureValue sv)
        {
             var idProp = sv.Properties.FirstOrDefault(p => p.Name == "Id");
             if (idProp != null && int.TryParse(idProp.Value.ToString(), out var id))
             {
                 eventId = id;
             }
        }

        var logModel = new LogModel
        {
            Timestamp = logEvent.Timestamp.DateTime,
            Level = level,
            Message = message,
            Exception = logEvent.Exception?.ToString(),
            Category = category,
            EventId = eventId
        };

        _logStore.AddLog(logModel);
    }
}
