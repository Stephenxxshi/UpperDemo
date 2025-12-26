using Plant01.Upper.Application.Contracts.DTOs;
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
    List<string> GetAllEquipmentCodes();

    /// <summary>
    /// 获取所有设备映射配置（用于触发标签扫描）
    /// </summary>
    List<EquipmentMappingDto> GetAllMappings();

    /// <summary>
    /// 批量获取设备
    /// </summary>
    List<Equipment> GetEquipmentsByRefs(List<string> refs);

    /// <summary>
    /// 获取设备的标签映射
    /// </summary>
    List<TagMappingDto> GetMappings(string equipmentCode);
}
