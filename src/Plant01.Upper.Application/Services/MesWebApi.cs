using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plant01.Domain.Shared.Interfaces;
using Plant01.Upper.Application.Interfaces;
using System.Text;

namespace Plant01.Upper.Application.Services;

/// <summary>
/// MES Web API 服务实现 - 接收MES生产工单推送
/// </summary>
public class MesWebApi : IMesWebApi
{
    private readonly IHttpService _httpService;
    private readonly ILogger<MesWebApi> _logger;
    private readonly string _baseUrl;
    private string? _basicAuthHeader;

    public MesWebApi(
        IHttpService httpService, 
        ILogger<MesWebApi> logger,
        IConfiguration configuration)
    {
        _httpService = httpService;
        _logger = logger;
        _baseUrl = configuration["MesWorkOrder:BaseUrl"] ?? "http://localhost:5000";
        
        // 从配置读取认证信息（如果有）
        var username = configuration["MesWorkOrder:Username"];
        var password = configuration["MesWorkOrder:Password"];
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            SetBasicAuth(username, password);
        }
    }

    /// <inheritdoc/>
    public async Task<WorkOrderResponse> CreateWorkOrderAsync(WorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            _logger.LogInformation("接收MES生产工单推送，工单号: {Code}, 产品: {ProductName}, 数量: {Quantity}, 状态: {Status}",
                request.Code, request.ProductName, request.Quantity, request.Status);

            // 设置 Basic 认证
            if (!string.IsNullOrEmpty(_basicAuthHeader))
            {
                _httpService.AddHeader("Authorization", _basicAuthHeader);
            }

            var url = $"{_baseUrl}/api/work_order/create";

            // 构建请求体（使用小写驼峰命名）
            var requestBody = new
            {
                code = request.Code,
                orderDate = request.OrderDate.ToString("yyyy-MM-dd"),
                lineNo = request.LineNo,
                productCode = request.ProductCode,
                productName = request.ProductName,
                productSpec = request.ProductSpec,
                quantity = request.Quantity,
                unit = request.Unit,
                batchNumber = request.BatchNumber,
                labelTemplateCode = request.LabelTemplateCode,
                status = request.Status,
                orderData = request.OrderData.Select(item => new
                {
                    key = item.Key,
                    name = item.Name,
                    value = item.Value
                }).ToList()
            };

            _logger.LogDebug("发送工单请求，URL: {Url}, 工单号: {Code}", url, request.Code);

            var response = await _httpService.PostJsonAsync<object, WorkOrderResponse>(url, requestBody, cancellationToken);

            if (response?.IsSuccess == true)
            {
                _logger.LogInformation("MES生产工单推送成功，工单号: {Code}", request.Code);
            }
            else
            {
                _logger.LogWarning("MES生产工单推送失败，工单号: {Code}, 错误码: {ErrorCode}, 错误信息: {ErrorMsg}",
                    request.Code, response?.ErrorCode, response?.ErrorMsg);
            }

            return response ?? new WorkOrderResponse { ErrorCode = -1, ErrorMsg = "响应为空" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MES生产工单推送异常，工单号: {Code}", request.Code);
            throw;
        }
    }

    /// <inheritdoc/>
    public void SetBasicAuth(string username, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // 生成 Basic 认证头：Basic base64(username:password)
        var credentials = $"{username}:{password}";
        var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        var base64Credentials = Convert.ToBase64String(credentialsBytes);
        _basicAuthHeader = $"Basic {base64Credentials}";

        _logger.LogDebug("已设置 Basic 认证，用户名: {Username}", username);
    }
}

#region 内部 DTO 模型

/// <summary>
/// 登录请求
/// </summary>
internal record LoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ClientType { get; init; } = string.Empty;
}

/// <summary>
/// 登录响应
/// </summary>
internal record LoginResponse
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? Message { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// 通用 API 响应
/// </summary>
internal record ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
}

#endregion
