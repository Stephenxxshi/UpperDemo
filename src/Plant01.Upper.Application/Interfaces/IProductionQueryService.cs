using Plant01.Upper.Application.DTOs;

namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// 生产数据查询服务接口
/// </summary>
public interface IProductionQueryService
{
    /// <summary>
    /// 获取最近的工单列表
    /// </summary>
    Task<List<WorkOrderDto>> GetRecentWorkOrdersAsync(int count = 10);

    /// <summary>
    /// 获取最近的包装袋记录
    /// </summary>
    Task<List<BagDto>> GetRecentBagsAsync(int count = 50);

    /// <summary>
    /// 获取最近的托盘记录
    /// </summary>
    Task<List<PalletDto>> GetRecentPalletsAsync(int count = 10);
}
