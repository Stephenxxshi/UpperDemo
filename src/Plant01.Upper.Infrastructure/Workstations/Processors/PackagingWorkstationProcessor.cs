using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 包装工位流程处理器示例
/// </summary>
public class PackagingWorkstationProcessor : IWorkstationProcessor
{
    public string WorkstationType => "Packaging";

    private readonly IDeviceCommunicationService _deviceComm;
    private readonly IMesService _mesService;
    private readonly ILogger<PackagingWorkstationProcessor> _logger;

    public PackagingWorkstationProcessor(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        ILogger<PackagingWorkstationProcessor> logger)
    {
        _deviceComm = deviceComm;
        _mesService = mesService;
        _logger = logger;
    }

    public async Task ExecuteAsync(WorkstationProcessContext context)
    {
        _logger.LogInformation("开始执行包装工位流程: {Workstation}, 触发标签: {Tag}",
            context.WorkstationCode, context.TriggerTagName);

        try
        {
            // 1. 读取订单号（假设有这个标签）
            var orderCodeTag = $"{context.EquipmentCode}.OrderCode";
            var orderCode = _deviceComm.GetTagValue<string>(orderCodeTag, string.Empty);

            if (string.IsNullOrEmpty(orderCode))
            {
                _logger.LogWarning("未读取到订单号，使用默认订单");
                orderCode = "DEFAULT_ORDER";
            }

            _logger.LogInformation("读取到订单号: {OrderCode}", orderCode);

            // 2. 查询MES订单信息（如果MES服务可用）
            try
            {
                // var orderInfo = await _mesService.GetOrderInfoAsync(orderCode);
                // if (orderInfo != null)
                // {
                //     // 3. 下发配方到PLC
                //     await _deviceComm.WriteTagAsync($"{context.EquipmentCode}.RecipeWeight", orderInfo.TargetWeight);
                //     await _deviceComm.WriteTagAsync($"{context.EquipmentCode}.RecipeSpeed", orderInfo.PackagingSpeed);
                //     _logger.LogInformation("配方已下发: 重量={Weight}, 速度={Speed}",
                //         orderInfo.TargetWeight, orderInfo.PackagingSpeed);
                // }

                _logger.LogInformation("订单信息查询完成（MES服务未实现，跳过）");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查询MES订单失败，继续执行");
            }

            // 4. 模拟配方下发
            await _deviceComm.WriteTagAsync($"{context.EquipmentCode}.RecipeWeight", 25.0f);
            await _deviceComm.WriteTagAsync($"{context.EquipmentCode}.RecipeSpeed", 60);
            _logger.LogInformation("默认配方已下发: 重量=25kg, 速度=60包/分钟");

            // 5. 等待PLC确认（可选）
            await Task.Delay(100);

            // 6. 写回成功结果
            await WriteResult(context.EquipmentCode, ProcessResult.Success, "流程执行成功");

            _logger.LogInformation("包装工位流程执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "包装工位流程执行失败");
            await WriteResult(context.EquipmentCode, ProcessResult.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 写回流程结果
    /// </summary>
    private async Task WriteResult(string equipmentCode, ProcessResult result, string message)
    {
        try
        {
            // 写回结果码
            var resultTag = $"{equipmentCode}.ProcessResult";
            await _deviceComm.WriteTagAsync(resultTag, (int)result);

            _logger.LogInformation("写回流程结果: {Tag} = {Result} ({Message})",
                resultTag, result, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写回流程结果失败");
        }
    }
}
