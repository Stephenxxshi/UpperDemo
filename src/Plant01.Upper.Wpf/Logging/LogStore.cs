using System;

namespace Plant01.Infrastructure.Shared.Logging
{
    public class LogStore : ILogStore
    {
        public event Action<LogModel>? LogAdded;
        public event Action? ClearRequested;
        private readonly object _lock = new();

        public int MaxItemCount { get; set; } = 2000;
        public bool IsPaused { get; set; }

        public void AddLog(LogModel log)
        {
            if (IsPaused) return;
            LogAdded?.Invoke(log);
        }

        public void Clear()
        {
            ClearRequested?.Invoke();
        }
    }
}