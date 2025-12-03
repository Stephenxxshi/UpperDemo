namespace Plant01.Upper.Application.Models.Logging;

public interface ILogStore
{
    event Action<LogModel>? LogAdded;
    event Action? ClearRequested;
    void AddLog(LogModel log);
    void Clear();
    int MaxItemCount { get; set; }
    bool IsPaused { get; set; }
}
