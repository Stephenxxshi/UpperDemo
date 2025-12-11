namespace Plant01.Upper.Application.Contracts.Api.Requests;

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
