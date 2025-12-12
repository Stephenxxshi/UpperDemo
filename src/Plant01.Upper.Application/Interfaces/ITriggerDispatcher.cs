using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Application.Interfaces;

public interface ITriggerDispatcher
{
    /// <summary>
    /// 将触发请求加入队列
    /// </summary>
    /// <param name="stationId">工站ID</param>
    /// <param name="source">触发源</param>
    /// <param name="payload">数据</param>
    /// <param name="priority">优先级</param>
    /// <param name="debounceKey">去抖键 (可选，相同Key在短时间内会被忽略)</param>
    Task EnqueueAsync(
        string stationId, 
        TriggerSourceType source, 
        string payload, 
        TriggerPriority priority = TriggerPriority.Normal, 
        string? debounceKey = null);
}
