using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// 设备配置服务接口
/// </summary>
public interface IEquipmentConfigService
{
    /// <summary>
    /// 根据设备编码获取设备实例（包含标签映射）
    /// </summary>
    Equipment? GetEquipment(string code);

    /// <summary>
    /// 获取所有设备编码
    /// </summary>
    IEnumerable<string> GetAllEquipmentCodes();
}
