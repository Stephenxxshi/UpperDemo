namespace Plant01.Upper.Application.Contracts.Api.Responses;

/// <summary>
/// 工单响应
/// </summary>
public record WorkOrderResponseDto
{
    /// <summary>
    /// 错误代码：0表示正确，其他代表具体错误编号
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
