using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class WsdomInkjetTcpDriver : IDriver
{
        private DeviceConfig? _config;
        private WsdomInkjetConfig? _driverConfig;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly ILogger<WsdomInkjetTcpDriver> _logger;

        // 缓存机制：Command -> (LastRequestTime, CachedData)
        private readonly Dictionary<string, (DateTime LastTime, object? Data)> _cache = new();
        
        // 刷新间隔配置：Command -> IntervalMs
        private Dictionary<string, int> _commandIntervals = new();

        public bool IsConnected => _client?.Connected ?? false;    public WsdomInkjetTcpDriver(ILogger<WsdomInkjetTcpDriver> logger)
    {
        _logger = logger;
    }

    public void Initialize(DeviceConfig config)
    {
        _config = config;
    }

    public void ValidateConfig(DeviceConfig config)
    {
        config.GetAndValidateDriverConfig<WsdomInkjetConfig>();
    }

        public async Task ConnectAsync()
        {
            if (_config == null) throw new InvalidOperationException("驱动未初始化");
            _driverConfig = _config.GetDriverConfig<WsdomInkjetConfig>();

            // 初始化刷新间隔配置
            _commandIntervals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["getPrintStatus"] = _driverConfig.StatusRefreshInterval,
                ["getJobs"] = _driverConfig.JobsRefreshInterval
            };

            // 合并自定义配置
            if (_driverConfig.CustomIntervals != null)
            {
                foreach (var kvp in _driverConfig.CustomIntervals)
                {
                    _commandIntervals[kvp.Key] = kvp.Value;
                }
            }

            try
            {
                _client = new TcpClient();
                using var cts = new CancellationTokenSource(_driverConfig.ConnectTimeout);
                await _client.ConnectAsync(_driverConfig.IpAddress, _driverConfig.Port, cts.Token);
                
                _stream = _client.GetStream();
                _client.ReceiveTimeout = _driverConfig.ReceiveTimeout;
                _client.SendTimeout = _driverConfig.ConnectTimeout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接喷码机失败: {Ip}:{Port}", _driverConfig.IpAddress, _driverConfig.Port);
                Dispose();
                throw;
            }
        }    public async Task DisconnectAsync()
    {
        _stream?.Close();
        _client?.Close();
        _client = null;
        _stream = null;
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
        _lock.Dispose();
    }

    public async Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        if (!IsConnected) throw new InvalidOperationException("设备未连接");

        var result = new Dictionary<string, object?>();
        var commTags = tags.OfType<CommunicationTag>().ToList();

        // 1. 聚合请求：根据地址前缀判断需要发送哪些命令
        bool needStatus = commTags.Any(t => t.Address.StartsWith("PrintStatus", StringComparison.OrdinalIgnoreCase));
        bool needJobs = commTags.Any(t => t.Address.StartsWith("Jobs", StringComparison.OrdinalIgnoreCase));

        try
        {
            await _lock.WaitAsync();

            // 2. 处理 getPrintStatus
            if (needStatus)
            {
                // 使用带缓存的请求方法
                var response = await GetCachedOrRequestAsync<PrintStatusData>("getPrintStatus");
                if (response != null)
                {
                    foreach (var tag in commTags.Where(t => t.Address.StartsWith("PrintStatus", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (tag.Address.Equals("PrintStatus", StringComparison.OrdinalIgnoreCase))
                            result[tag.Name] = JsonSerializer.Serialize(response);
                        else if (tag.Address.Equals("PrintStatus.Progress", StringComparison.OrdinalIgnoreCase))
                            result[tag.Name] = response.Progress;
                        else if (tag.Address.Equals("PrintStatus.TotalPages", StringComparison.OrdinalIgnoreCase))
                            result[tag.Name] = response.TotalPages;
                    }
                }
            }

            // 3. 处理 getJobs
            if (needJobs)
            {
                // 使用带缓存的请求方法
                var response = await GetCachedOrRequestAsync<List<JobData>>("getJobs");
                if (response != null)
                {
                    foreach (var tag in commTags.Where(t => t.Address.StartsWith("Jobs", StringComparison.OrdinalIgnoreCase)))
                    {
                        result[tag.Name] = JsonSerializer.Serialize(response);
                    }
                }
            }
        }
        finally
        {
            _lock.Release();
        }

        return result;
    }

    public async Task WriteTagAsync(object tag, object value)
    {
        if (!IsConnected) throw new InvalidOperationException("设备未连接");
        var commTag = tag as CommunicationTag ?? throw new ArgumentException("无效的标签类型");

        try
        {
            await _lock.WaitAsync();

            if (commTag.Address.Equals("FlashJet", StringComparison.OrdinalIgnoreCase))
            {
                // 支持传入 JSON 字符串或对象
                object? data = value is string jsonStr
                    ? JsonSerializer.Deserialize<FlashJetData>(jsonStr)
                    : value;

                await SendCommandAsync<object>("flashJet", data);
            }
            else if (commTag.Address.Equals("UpdateJob", StringComparison.OrdinalIgnoreCase))
            {
                object? data = value is string jsonStr
                    ? JsonSerializer.Deserialize<UpdateJobData>(jsonStr)
                    : value;

                await SendCommandAsync<object>("updateJob", data);
            }
            else
            {
                throw new NotSupportedException($"不支持写入地址: {commTag.Address}");
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 获取缓存数据或发送请求
    /// </summary>
    private async Task<TData?> GetCachedOrRequestAsync<TData>(string command)
    {
        // 确定刷新间隔
        if (!_commandIntervals.TryGetValue(command, out int intervalMs))
        {
            intervalMs = 0;
        }

        // 检查缓存
        if (intervalMs > 0 && _cache.TryGetValue(command, out var cacheEntry))
        {
            if ((DateTime.Now - cacheEntry.LastTime).TotalMilliseconds < intervalMs)
            {
                return (TData?)cacheEntry.Data;
            }
        }

        // 发送请求
        var data = await SendCommandAsync<TData>(command);

        // 更新缓存
        if (intervalMs > 0 && data != null)
        {
            _cache[command] = (DateTime.Now, data);
        }

        return data;
    }

    /// <summary>
    /// 发送命令并接收响应
    /// </summary>
    private async Task<TData?> SendCommandAsync<TData>(string command, object? data = null)
    {
        if (_stream == null) throw new InvalidOperationException("网络流未初始化");

        // 1. 构建请求包
        var request = new WsdomRequest { Command = command, Data = data };
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(request);

        // 包长度 = 1 (Header) + 2 (Length) + N (Data)
        int totalLength = 1 + 2 + jsonBytes.Length;
        if (totalLength > 65535) throw new ArgumentException("数据包过大");

        var buffer = new byte[totalLength];
        buffer[0] = 0x8C; // Header

        // Length (Big Endian)
        var lenBytes = BitConverter.GetBytes((ushort)totalLength);
        if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
        Array.Copy(lenBytes, 0, buffer, 1, 2);

        // Data
        Array.Copy(jsonBytes, 0, buffer, 3, jsonBytes.Length);

        // 2. 发送
        await _stream.WriteAsync(buffer);

        // 3. 接收响应头 (3字节)
        var headerBuffer = new byte[3];
        int read = await ReadExactAsync(headerBuffer, 3);
        if (read != 3) throw new IOException("读取响应头失败");

        if (headerBuffer[0] != 0x8C) throw new IOException($"无效的包头: {headerBuffer[0]:X2}");

        // 解析长度
        var respLenBytes = new byte[2];
        Array.Copy(headerBuffer, 1, respLenBytes, 0, 2);
        if (BitConverter.IsLittleEndian) Array.Reverse(respLenBytes);
        ushort respTotalLength = BitConverter.ToUInt16(respLenBytes, 0);

        // 4. 接收响应体
        int bodyLength = respTotalLength - 3;
        if (bodyLength < 0) throw new IOException("无效的包长度");

        var bodyBuffer = new byte[bodyLength];
        if (bodyLength > 0)
        {
            await ReadExactAsync(bodyBuffer, bodyLength);
        }

        // 5. 解析 JSON
        var jsonResponse = JsonSerializer.Deserialize<WsdomResponse<TData>>(bodyBuffer);
        if (jsonResponse == null) throw new IOException("响应反序列化失败");

        if (jsonResponse.Code != 0)
        {
            _logger.LogWarning("设备返回错误: {Message} (Code: {Code})", jsonResponse.Message, jsonResponse.Code);
            // 根据业务需求，这里可以选择抛出异常或返回 null
            throw new IOException($"设备操作失败: {jsonResponse.Message}");
        }

        return jsonResponse.Data;
    }

    private async Task<int> ReadExactAsync(byte[] buffer, int count)
    {
        if (_stream == null) return 0;
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = await _stream.ReadAsync(buffer, totalRead, count - totalRead);
            if (read == 0) break;
            totalRead += read;
        }
        return totalRead;
    }



    #region Protocol Models

    private class WsdomRequest
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }
    }

    private class WsdomResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    // Data Models
    public class PrintStatusData
    {
        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }

    public class JobData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("variables")]
        public List<string> Variables { get; set; } = new();
    }

    public class FlashJetData
    {
        [JsonPropertyName("frequence")]
        public int Frequence { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class UpdateJobData
    {
        [JsonPropertyName("job")]
        public string Job { get; set; } = string.Empty;
        // 其他字段根据需要添加...
    }

    #endregion
}
