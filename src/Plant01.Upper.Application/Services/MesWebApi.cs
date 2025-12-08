using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using System.Text;

namespace Plant01.Upper.Application.Services;

/// <summary>
/// MES Web API 服务实现 - 接收MES生产工单推送 (Server Mode)
/// </summary>
public class MesWebApi : IMesWebApi
{
    private readonly ILogger<MesWebApi> _logger;
    private readonly string _listenUrl;
    private readonly string? _expectedUsername;
    private readonly string? _expectedPassword;
    private WebApplication? _app;

    public event Func<WorkOrderRequestDto, Task<WorkOrderResponse>>? OnWorkOrderReceived;
    public bool IsRunning => _app != null;

    public MesWebApi(
        ILogger<MesWebApi> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        // 默认监听端口 5000，可配置
        _listenUrl = configuration["MesWorkOrder:ListenUrl"] ?? "http://0.0.0.0:5000";
        
        // 从配置读取期望的认证信息（用于验证客户端）
        _expectedUsername = configuration["MesWorkOrder:Username"];
        _expectedPassword = configuration["MesWorkOrder:Password"];
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_app != null) return;

        try
        {
            var builder = WebApplication.CreateBuilder();
            
            // 配置 Kestrel 监听地址
            builder.WebHost.UseUrls(_listenUrl);
            
            // 配置 JSON 序列化选项 (忽略大小写)
            builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });

            // 添加日志
            builder.Services.AddLogging(logging => 
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

            _app = builder.Build();

            // 映射 POST 接口
            _app.MapPost("/api/work_order/create", HandleCreateWorkOrder);

            await _app.StartAsync(cancellationToken);
            _logger.LogInformation("MES Web API 服务已启动，监听地址: {Url}", _listenUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动 MES Web API 服务失败");
            _app = null;
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_app == null) return;

        try
        {
            await _app.StopAsync(cancellationToken);
            await _app.DisposeAsync();
            _app = null;
            _logger.LogInformation("MES Web API 服务已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止 MES Web API 服务失败");
        }
    }

    private async Task<IResult> HandleCreateWorkOrder(HttpContext context, [FromBody] WorkOrderRequestDto request)
    {
        _logger.LogInformation("收到工单推送请求: {Code}", request.Code);

        // Basic Auth 验证
        if (!string.IsNullOrEmpty(_expectedUsername) && !string.IsNullOrEmpty(_expectedPassword))
        {
            if (!ValidateBasicAuth(context.Request.Headers["Authorization"]))
            {
                _logger.LogWarning("工单推送认证失败");
                return Results.Unauthorized();
            }
        }

        if (OnWorkOrderReceived != null)
        {
            try 
            {
                var response = await OnWorkOrderReceived.Invoke(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理工单推送时发生异常");
                return Results.Json(new WorkOrderResponse { ErrorCode = 500, ErrorMsg = $"内部错误: {ex.Message}" });
            }
        }

        return Results.Ok(new WorkOrderResponse { ErrorCode = -1, ErrorMsg = "服务未就绪 (No Handler)" });
    }

    private bool ValidateBasicAuth(string? authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var encoded = authHeader.Substring(6).Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);
            if (parts.Length != 2) return false;

            return parts[0] == _expectedUsername && parts[1] == _expectedPassword;
        }
        catch
        {
            return false;
        }
    }
}
