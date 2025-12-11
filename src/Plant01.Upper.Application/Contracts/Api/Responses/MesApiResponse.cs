namespace Plant01.Upper.Application.Contracts.Api.Responses;

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
