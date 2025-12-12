namespace Plant01.Upper.Application.Models;

public enum TriggerSourceType
{
    PLC,        // PLC 信号
    Scanner,    // 扫码枪
    Manual,     // 人工/按钮
    System      // 系统内部触发
}

public enum TriggerPriority
{
    Normal = 0, // 普通优先级 (丢弃旧数据)
    High = 1    // 高优先级 (阻塞等待，不丢弃)
}
