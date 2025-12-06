namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// PLC 流程控制服务接口
/// </summary>
public interface IPlcFlowService
{
    /// <summary>
    /// 处理上袋请求
    /// </summary>
    Task<bool> ProcessLoadingRequestAsync(string bagCode, string machineId);

    /// <summary>
    /// 处理套袋请求
    /// </summary>
    Task<bool> ProcessBaggingRequestAsync(string bagCode, string machineId);

    /// <summary>
    /// 处理装料/包装请求
    /// </summary>
    Task<bool> ProcessFillingRequestAsync(string bagCode, string machineId);

    /// <summary>
    /// 处理复检称重请求
    /// </summary>
    /// <param name="weight">实际重量</param>
    Task<bool> ProcessWeighingRequestAsync(string bagCode, string machineId, double weight);

    /// <summary>
    /// 处理喷码请求
    /// </summary>
    /// <returns>允许喷码时返回喷码内容，否则返回空</returns>
    Task<string?> ProcessPrintingRequestAsync(string bagCode, string machineId);

    /// <summary>
    /// 处理码垛请求
    /// </summary>
    Task<bool> ProcessPalletizingRequestAsync(string bagCode, string palletCode, string machineId, int positionIndex);

    /// <summary>
    /// 处理出垛请求
    /// </summary>
    Task<bool> ProcessPalletOutRequestAsync(string palletCode, string machineId);
}
