namespace Plant01.Upper.Application.Contracts.Api.Requests;

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
    public required string JobNo { get; init; }

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
