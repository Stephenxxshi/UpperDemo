namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// MES 服务接口
/// </summary>
public interface IMesService
{
    /// <summary>
    /// 锐派码垛完成
    /// </summary>
    /// <param name="request">码垛完成请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    Task<MesApiResponse> FinishPalletizingAsync(FinishPalletizingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 锐派托盘缺少
    /// </summary>
    /// <param name="request">托盘缺少请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    Task<MesApiResponse> ReportLackPalletAsync(LackPalletRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 锐派码垛完成请求
/// </summary>
public record FinishPalletizingRequest
{
    /// <summary>
    /// AGV设备标识，非空
    /// </summary>
    public required string AgvDeviceCode { get; init; }

    /// <summary>
    /// 共享托盘ID，非空
    /// </summary>
    public required string PalletId { get; init; }

    /// <summary>
    /// 码垛机设备标识，非空
    /// </summary>
    public required string DeviceCode { get; init; }

    /// <summary>
    /// MES生产任务ID，非空
    /// </summary>
    public required int JobId { get; init; }

    /// <summary>
    /// 打包明细，非空
    /// </summary>
    public required List<PackageDetail> List { get; init; }
}

/// <summary>
/// 打包明细
/// </summary>
public record PackageDetail
{
    /// <summary>
    /// 包号
    /// </summary>
    public required string BagNums { get; init; }

    /// <summary>
    /// 数量
    /// </summary>
    public required decimal Quan { get; init; }
}

/// <summary>
/// 锐派托盘缺少请求
/// </summary>
public record LackPalletRequest
{
    /// <summary>
    /// AGV设备标识，非空
    /// </summary>
    public required string AgvDeviceCode { get; init; }

    /// <summary>
    /// 托盘类型 1.表示母托盘，2表示子托盘，非空
    /// </summary>
    public required int PalletType { get; init; }
}

/// <summary>
/// MES API 响应
/// </summary>
public record MesApiResponse
{
    /// <summary>
    /// 错误代码：0代表正确，其它数代表不同的错误标识
    /// </summary>
    public int ErrorCode { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMsg { get; init; } = string.Empty;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => ErrorCode == 0;
}
