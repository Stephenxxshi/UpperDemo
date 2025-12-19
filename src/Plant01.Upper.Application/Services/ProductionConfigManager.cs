using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Application.Services;

/// <summary>
/// 产线配置内存管理器（不持久化到数据库）
/// 所有产线/工段/工位/设备信息在内存中维护，通过配置文件初始化
/// </summary>
public class ProductionConfigManager
{
    private readonly ILogger<ProductionConfigManager> _logger;
    private List<ProductionLine> _productionLines = new();

    public ProductionConfigManager(ILogger<ProductionConfigManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 从配置加载产线配置到内存
    /// </summary>
    public void LoadFromConfig(List<ProductionLine> lines)
    {
        _productionLines = lines;
        _logger.LogInformation($"已加载 {lines.Count} 条产线到内存，共 {CountWorkstations()} 个工位，{CountEquipments()} 个设备");
    }

    /// <summary>
    /// 根据产线Code获取产线
    /// </summary>
    public ProductionLine? GetProductionLineByCode(string code)
    {
        return _productionLines.FirstOrDefault(x => x.Code == code);
    }

    /// <summary>
    /// 获取所有产线
    /// </summary>
    public List<ProductionLine> GetAllProductionLines()
    {
        return _productionLines;
    }

    /// <summary>
    /// 根据工位Code获取工位
    /// </summary>
    public Workstation? GetWorkstationByCode(string workstationCode)
    {
        return _productionLines
            .SelectMany(x => x.Workstations)
            .FirstOrDefault(x => x.Code == workstationCode);
    }

    /// <summary>
    /// 根据设备Code获取设备
    /// </summary>
    public Equipment? GetEquipmentByCode(string equipmentCode)
    {
        return _productionLines
            .SelectMany(x => x.Workstations)
            .SelectMany(x => x.Equipments)
            .FirstOrDefault(x => x.Code == equipmentCode);
    }

    /// <summary>
    /// 获取某工位的所有设备
    /// </summary>
    public List<Equipment> GetEquipmentsByWorkstation(string workstationCode)
    {
        var workstation = GetWorkstationByCode(workstationCode);
        return workstation?.Equipments ?? new List<Equipment>();
    }

    /// <summary>
    /// 获取工位所属的产线
    /// </summary>
    public ProductionLine? GetProductionLineByWorkstation(string workstationCode)
    {
        return _productionLines.FirstOrDefault(x => x.Workstations.Any(w => w.Code == workstationCode));
    }

    /// <summary>
    /// 获取设备所属的工位
    /// </summary>
    public Workstation? GetWorkstationByEquipment(string equipmentCode)
    {
        return _productionLines
            .SelectMany(x => x.Workstations)
            .FirstOrDefault(x => x.Equipments.Any(e => e.Code == equipmentCode));
    }

    /// <summary>
    /// 获取设备所属的产线
    /// </summary>
    public ProductionLine? GetProductionLineByEquipment(string equipmentCode)
    {
        var workstation = GetWorkstationByEquipment(equipmentCode);
        return workstation != null ? GetProductionLineByWorkstation(workstation.Code) : null;
    }

    /// <summary>
    /// 获取产线的分配策略（JSON字符串）
    /// </summary>
    public string? GetLineStrategyJson(string lineCode)
    {
        var line = GetProductionLineByCode(lineCode);
        return line?.StrategyConfigJson;
    }

    /// <summary>
    /// 统计总工位数
    /// </summary>
    private int CountWorkstations()
    {
        return _productionLines.Sum(x => x.Workstations.Count);
    }

    /// <summary>
    /// 统计总设备数
    /// </summary>
    private int CountEquipments()
    {
        return _productionLines.Sum(x => x.Workstations.Sum(w => w.Equipments.Count));
    }

    /// <summary>
    /// 获取配置统计信息（用于调试和日志）
    /// </summary>
    public string GetConfigSummary()
    {
        return $"产线数: {_productionLines.Count}, 工位数: {CountWorkstations()}, 设备数: {CountEquipments()}";
    }
}
