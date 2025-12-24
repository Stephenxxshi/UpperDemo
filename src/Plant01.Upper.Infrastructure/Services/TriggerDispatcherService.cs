using System.Collections.Concurrent;
using System.Threading.Channels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Messages;
using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Infrastructure.Services;

public class TriggerDispatcherService : BackgroundService, ITriggerDispatcher
{
    private readonly ILogger<TriggerDispatcherService> _logger;
    private readonly Channel<StationTriggerMessage> _highPriorityChannel;
    private readonly Channel<StationTriggerMessage> _normalPriorityChannel;
    
    // 去抖动存储: Key -> LastTicks
    private readonly ConcurrentDictionary<string, long> _debounceStore = new();
    private readonly int _debounceMs;

    public TriggerDispatcherService(ILogger<TriggerDispatcherService> logger, IConfiguration configuration)
    {
        _logger = logger;
        // 从配置读取去抖时间，默认 500ms
        _debounceMs = configuration.GetValue<int>("TriggerSettings:DebounceMs", 500);

        // 高优先级通道: 满载时等待 (Wait)，确保关键信号不丢失
        _highPriorityChannel = Channel.CreateBounded<StationTriggerMessage>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        // 普通优先级通道: 满载时丢弃旧数据 (DropOldest)，防止积压导致系统卡顿
        _normalPriorityChannel = Channel.CreateBounded<StationTriggerMessage>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async Task EnqueueAsync(string stationId, TriggerSourceType source, string payload, TriggerPriority priority = TriggerPriority.Normal, string? debounceKey = null)
    {
        // 1. 去抖动检查
        if (!string.IsNullOrEmpty(debounceKey))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (_debounceStore.TryGetValue(debounceKey, out var lastTime))
            {
                if (now - lastTime < _debounceMs)
                {
                    _logger.LogDebug("触发器去抖: {Key}", debounceKey);
                    return; // 忽略此次触发
                }
            }
            _debounceStore[debounceKey] = now;
            // todo：长期运行可能需要清理过期的 Key，这里暂略
        }

        // 2. 构建消息 (生成 TraceId)
        var message = new StationTriggerMessage(
            TraceId: Guid.NewGuid().ToString("N"),
            StationId: stationId,
            Source: source,
            Payload: payload,
            Priority: priority,
            Timestamp: DateTime.UtcNow
        );

        // 3. 写入对应通道
        var channel = priority == TriggerPriority.High ? _highPriorityChannel : _normalPriorityChannel;
        await channel.Writer.WriteAsync(message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("触发器调度服务已启动。");

        // 并行处理两个通道
        var highTask = ProcessChannelAsync(_highPriorityChannel.Reader, stoppingToken);
        var normalTask = ProcessChannelAsync(_normalPriorityChannel.Reader, stoppingToken);

        await Task.WhenAll(highTask, normalTask);
    }

    private async Task ProcessChannelAsync(ChannelReader<StationTriggerMessage> reader, CancellationToken ct)
    {
        await foreach (var msg in reader.ReadAllAsync(ct))
        {
            // 4. 注入 TraceId 到日志上下文
            using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = msg.TraceId }))
            {
                try
                {
                    _logger.LogInformation("调度触发器: 站点={StationId}, 负载={Payload}", msg.StationId, msg.Payload);
                    
                    // 5. 广播消息 (同步调用所有订阅者)
                    WeakReferenceMessenger.Default.Send(msg);
                }
                catch (Exception ex)
                {
                    // 6. 异常隔离: 防止单个业务逻辑崩溃导致整个监控服务停止
                    _logger.LogError(ex, "处理 {StationId} 的触发器时出错", msg.StationId);
                }
            }
        }
    }
}
