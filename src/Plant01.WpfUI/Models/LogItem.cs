using System;

namespace Plant01.WpfUI.Models
{
    public class LogItem
    {
        public DateTime Timestamp { get; set; }
        public LogSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}