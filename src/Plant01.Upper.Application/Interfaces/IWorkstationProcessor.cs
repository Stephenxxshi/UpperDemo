namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// 工位流程处理器接口
/// </summary>
public interface IWorkstationProcessor
{
    /// <summary>
    /// 工位类型（如 Packaging, Palletizing）
    /// </summary>
    string WorkstationType { get; }
    
    /// <summary>
    /// 执行工位流程
    /// </summary>
    Task ExecuteAsync(WorkstationProcessContext context);
}

/// <summary>
/// 工位流程上下文
/// </summary>
public class WorkstationProcessContext
{
    /// <summary>
    /// 工位代码
    /// </summary>
    public string WorkstationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 触发的设备代码
    /// </summary>
    public string EquipmentCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 触发标签名称
    /// </summary>
    public string TriggerTagName { get; set; } = string.Empty;
    
    /// <summary>
    /// 触发值
    /// </summary>
    public object? TriggerValue { get; set; }
    
    /// <summary>
    /// 触发时间
    /// </summary>
    public DateTime TriggerTime { get; set; }
    
    /// <summary>
    /// 扩展参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// 流程执行结果枚举
/// </summary>
public enum ProcessResult
{
    /// <summary>
    /// 待机
    /// </summary>
    Idle = 0,
    
    /// <summary>
    /// 成功
    /// </summary>
    Success = 1,
    
    /// <summary>
    /// 错误
    /// </summary>
    Error = 2,
    
    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 3
}
