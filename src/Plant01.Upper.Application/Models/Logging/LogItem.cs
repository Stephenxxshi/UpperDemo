using Microsoft.Extensions.Logging;

using System;

namespace Plant01.Upper.Application.Models.Logging
{
    public enum LogSeverity
    {
        Info,
        Debug,
        Warning,
        Error
    }

    public class LogItem
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
