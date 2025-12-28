namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 设备-标签映射（将业务设备与通信标签关联）
/// 纯内存对象，从配置文件加载，不持久化到数据库
/// </summary>
public class EquipmentTagMapping
{
    /// <summary>
    /// 标签代码（通信层标签全名，如 SDJ01.Heartbeat）
    /// </summary>
    public string TagCode { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称（业务层标签全名，如 包装机01_申请读码）
    /// </summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签用途（如 Heartbeat, Alarm, Output, Mode, Recipe 等）
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
    
    /// <summary>
    /// 关联的通道名称（可选，用于快速定位）
    /// </summary>
    public string? ChannelName { get; set; }
    
    /// <summary>
    /// 是否为关键标签（用于监控优先级）
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// 标签方向（输入/输出）
    /// </summary>
    public TagDirection Direction { get; set; } = TagDirection.Input;
    
    /// <summary>
    /// 是否为触发标签（用于工位流程触发）
    /// </summary>
    public bool IsTrigger { get; set; }
    
    /// <summary>
    /// 触发条件表达式（如 "== true", "> 0"）
    /// </summary>
    public string? TriggerCondition { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 标签方向枚举
/// </summary>
public enum TagDirection
{
    /// <summary>
    /// 输入标签（PLC → 上位机）：触发信号、状态反馈
    /// </summary>
    Input = 0,
    
    /// <summary>
    /// 输出标签（上位机 → PLC）：控制指令、结果回写
    /// </summary>
    Output = 1
}

/// <summary>
/// 标签用途枚举（常见用途预定义）
/// </summary>
public static class TagPurpose
{
    public const string Heartbeat = "Heartbeat";      // 心跳
    public const string Alarm = "Alarm";              // 报警
    public const string AlarmCode = "AlarmCode";      // 报警码
    public const string OutputCount = "OutputCount";  // 产量
    public const string Mode = "Mode";                // 模式
    public const string Status = "Status";            // 状态
    public const string Recipe = "Recipe";            // 配方
    public const string Quality = "Quality";          // 质量
    public const string Power = "Power";              // 电源状态
    public const string Speed = "Speed";              // 速度
    public const string Temperature = "Temperature";  // 温度
    public const string Pressure = "Pressure";        // 压力
    public const string QrCode = "QrCode";                // 二维码
    
    // === 流程控制相关 ===
    public const string ProcessTrigger = "ProcessTrigger";     // 流程触发信号
    public const string ProcessResult = "ProcessResult";       // 流程结果回写
    public const string OrderRequest = "OrderRequest";         // 订单请求
    public const string RecipeDownload = "RecipeDownload";     // 配方下发
    public const string Ready = "Ready";                       // 就绪信号
    public const string Busy = "Busy";                         // 忙碌信号
    public const string Complete = "Complete";                 // 完成信号
    public const string BarcodeRead = "BarcodeRead";           // 条码读取
}
