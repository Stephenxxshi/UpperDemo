using Microsoft.Extensions.Logging;

namespace Plant01.Upper.Application.Models.Logging;

public class LogModel
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string Category { get; set; } = string.Empty;
    public int EventId { get; set; }
}
