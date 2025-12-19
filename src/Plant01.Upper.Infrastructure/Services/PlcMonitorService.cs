using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// PLC ��ط���
/// </summary>
public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IDeviceCommunicationService _deviceService;

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IDeviceCommunicationService deviceService)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _deviceService = deviceService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC ��ط������������Ž�ģʽ����");
        
        // ���ı�ǩ�仯�¼�
        _deviceService.TagChanged += OnTagChanged;
        
        return Task.CompletedTask;
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        // ����Ҫ����������ر�ǩ
        // Ŀǰ�����Ǽ����κα�ǩ�仯���Ǵ����������ݸ���
        
        // ʾ��ӳ���߼���
        // �����ǩ������ "ST" ��ͷ����������վ�㴥����
        // ���� "ST01_Loading.Trigger"
        
        try
        {
            // ���߼���Ŀǰֻ��ת��һ�У����ض���ǩ����
            // ����ʵӦ���У������� "TriggerMap" �в��ұ�ǩ
            
            // ʾ�������ֵΪ���� TRUE���򴥷��¼�
            if (e.NewValue.Value is bool bVal && bVal)
            {
                // �� TagName ����ȡ StationId������ "SDJ01.HeartBreak" -> "SDJ01"��
                var parts = e.TagName.Split('.');
                var stationId = parts.Length > 0 ? parts[0] : "Unknown";
                
                await _dispatcher.EnqueueAsync(
                    stationId: stationId,
                    source: TriggerSourceType.PLC,
                    payload: $"{e.TagName}={e.NewValue.Value}",
                    priority: TriggerPriority.Normal,
                    debounceKey: e.TagName // ����ǩ����ȥ��
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "������ǩ�仯ʱ���� {Tag}", e.TagName);
        }
    }

    public override void Dispose()
    {
        _deviceService.TagChanged -= OnTagChanged;
        base.Dispose();
    }
}
