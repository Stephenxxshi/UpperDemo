namespace Plant01.Domain.Shared.Models.Equipment;

[Flags]
public enum Capabilities
{
    None = 0,
    Heartbeat = 1 << 0,
    AlarmReport = 1 << 1,
    OutputCount = 1 << 2,
    ModeStatus = 1 << 3,
    RecipeDownload = 1 << 4,
    ParameterReadWrite = 1 << 5,
    QualityCheck = 1 << 6,
    PowerStatus = 1 << 7
}
