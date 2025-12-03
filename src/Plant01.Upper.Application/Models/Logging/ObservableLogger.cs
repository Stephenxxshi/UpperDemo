using Microsoft.Extensions.Logging;

namespace Plant01.Upper.Application.Models.Logging;

public class ObservableLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ILogStore _logStore;

    public ObservableLogger(string categoryName, ILogStore logStore)
    {
        _categoryName = categoryName;
        _logStore = logStore;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);

        var logModel = new LogModel
        {
            Timestamp = DateTime.Now,
            Level = logLevel,
            Message = message,
            Exception = exception?.ToString(),
            Category = _categoryName,
            EventId = eventId.Id
        };

        _logStore.AddLog(logModel);
    }
}