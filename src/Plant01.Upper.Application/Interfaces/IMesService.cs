using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.Api.Responses;

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
    /// 简化的码垛完成上报 (适配 Domain Event)
    /// </summary>
    Task ReportPalletCompletionAsync(string workOrderCode, string palletCode);

    /// <summary>
    /// 锐派托盘缺少
    /// </summary>
    /// <param name="request">托盘缺少请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    Task<MesApiResponse> ReportLackPalletAsync(LackPalletRequest request, CancellationToken cancellationToken = default);
}








