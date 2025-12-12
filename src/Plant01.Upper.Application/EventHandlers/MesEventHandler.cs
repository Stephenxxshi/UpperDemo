using Microsoft.Extensions.Logging;
using Plant01.Domain.Shared.Events;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Events;

namespace Plant01.Upper.Application.EventHandlers;

public class MesEventHandler
{
    private readonly IMesService _mesService;
    private readonly ILogger<MesEventHandler> _logger;

    public MesEventHandler(IMesService mesService, ILogger<MesEventHandler> logger)
    {
        _mesService = mesService;
        _logger = logger;
    }

    public async Task HandlePalletDischargedAsync(PalletDischargedEvent domainEvent)
    {
        _logger.LogInformation("Handling PalletDischargedEvent for {PalletCode}", domainEvent.PalletCode);

        try
        {
            // 调用 MES 接口报工
            // 注意：这里假设 IMesService 有对应的方法，如果没有需要添加
            await _mesService.ReportPalletCompletionAsync(domainEvent.WorkOrderCode, domainEvent.PalletCode);
            
            _logger.LogInformation("Successfully reported pallet {PalletCode} to MES", domainEvent.PalletCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report pallet {PalletCode} to MES", domainEvent.PalletCode);
            // 考虑重试机制或写入死信队列
        }
    }
}
