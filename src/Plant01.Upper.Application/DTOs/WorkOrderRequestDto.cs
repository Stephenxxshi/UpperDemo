namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// 生产工单推送请求
/// </summary>
public record WorkOrderRequestDto
{
    /// <summary>
    /// 生产工单号
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 工单日期
    /// </summary>
    public required DateTime OrderDate { get; init; }

    /// <summary>
    /// 生产产线编号
    /// </summary>
    public required string LineNo { get; init; }

    /// <summary>
    /// 产品编号
    /// </summary>
    public required string ProductCode { get; init; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public required string ProductName { get; init; }

    /// <summary>
    /// 规格型号
    /// </summary>
    public string ProductSpec { get; init; } = string.Empty;

    /// <summary>
    /// 计划生产数量
    /// </summary>
    public required decimal Quantity { get; init; }

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// 批号
    /// </summary>
    public string BatchNumber { get; init; } = string.Empty;

    /// <summary>
    /// 标签模板编号
    /// </summary>
    public string LabelTemplateCode { get; init; } = string.Empty;

    /// <summary>
    /// 状态：1开工 99完工
    /// </summary>
    public required int Status { get; init; }

    /// <summary>
    /// 工单数据
    /// </summary>
    public List<OrderDataItem> OrderData { get; init; } = new();
}

/// <summary>
/// 工单数据项
/// </summary>
public record OrderDataItem
{
    /// <summary>
    /// 键
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 值
    /// </summary>
    public required string Value { get; init; }
}

/// <summary>
/// 工单响应
/// </summary>
public record WorkOrderResponse
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
