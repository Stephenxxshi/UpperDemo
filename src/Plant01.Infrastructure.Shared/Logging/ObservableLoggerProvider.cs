using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Plant01.Infrastructure.Shared.Logging
{
    public class ObservableLoggerProvider : ILoggerProvider
    {
        private readonly ILogStore _logStore;
        private readonly ConcurrentDictionary<string, ObservableLogger> _loggers = new();

        public ObservableLoggerProvider(ILogStore logStore)
        {
            _logStore = logStore;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ObservableLogger(name, _logStore));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}