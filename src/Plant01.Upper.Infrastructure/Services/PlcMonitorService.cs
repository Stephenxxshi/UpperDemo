using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Infrastructure.Services;

public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IConfiguration _configuration;
    
    // 模拟 PLC 信号存储 (实际应替换为 HslCommunication 或 S7NetPlus 实例)
    // Key: StationId, Value: SignalAddress
    private readonly Dictionary<string, string> _plcSignals = new();

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IConfiguration configuration)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _configuration = configuration;

        // 从配置加载 PLC 信号映射
        // 假设配置结构: "PlcSettings": { "Signals": { "ST01_Loading": "M100", ... } }
        var signals = configuration.GetSection("PlcSettings:Signals").Get<Dictionary<string, string>>();
        if (signals != null)
        {
            _plcSignals = signals;
        }

        // 如果配置为空，添加一些示例数据
        if (_plcSignals.Count == 0)
        {
            _plcSignals.Add("ST01_Loading", "M100");
            _plcSignals.Add("ST02_Unloading", "M101");
            _plcSignals.Add("ST03_Assembly", "M102");
            _plcSignals.Add("ST04_Weighing", "M103");
            _plcSignals.Add("ST05_Packaging", "M104");
            _plcSignals.Add("ST06_QualityCheck", "M105");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Monitor Service Started.");

        // 模拟 PLC 连接
        // ConnectPlc();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var station in _plcSignals)
                {
                    string stationId = station.Key;
                    string address = station.Value;

                    // 1. 读取 PLC 信号 (模拟)
                    // bool isTriggered = _plc.ReadBool(address);
                    bool isTriggered = MockReadPlc(address); 

                    if (isTriggered)
                    {
                        // 2. 读取附带数据 (如条码)
                        // string barcode = _plc.ReadString(address + "_DATA", 20);
                        string payload = MockReadPayload(stationId);

                        // 3. 分发触发请求
                        // 使用 StationId + Address 作为去抖键
                        string debounceKey = $"PLC_{stationId}_{address}";
                        
                        await _dispatcher.EnqueueAsync(
                            stationId, 
                            TriggerSourceType.PLC, 
                            payload, 
                            TriggerPriority.Normal, 
                            debounceKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring PLC signals");
            }

            // 轮询间隔
            await Task.Delay(100, stoppingToken);
        }
    }

    // --- 模拟方法 ---

    private readonly Random _random = new();
    private bool MockReadPlc(string address)
    {
        // 随机触发，仅用于演示
        return _random.Next(0, 100) > 98; 
    }

    private string MockReadPayload(string stationId)
    {
        return stationId switch
        {
            "ST04_Weighing" => $"BAG{_random.Next(1000, 9999)}:{_random.Next(49, 51)}.{_random.Next(0, 9)}", // 模拟称重数据
            _ => $"BAG{_random.Next(1000, 9999)}"
        };
    }
}
