using System;

namespace Plant01.Infrastructure.Shared.Logging
{
    public interface ILogStore
    {
        event Action<LogModel>? LogAdded;
        event Action? ClearRequested;
        void AddLog(LogModel log);
        void Clear();
        int MaxItemCount { get; set; }
        bool IsPaused { get; set; }
    }
}