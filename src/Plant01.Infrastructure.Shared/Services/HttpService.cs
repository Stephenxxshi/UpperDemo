using Microsoft.Extensions.Logging;
using Plant01.Domain.Shared.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Plant01.Infrastructure.Shared.Services;

/// <summary>
/// HTTP 服务实现 - 使用 IHttpClientFactory 管理生命周期
/// </summary>
public sealed class HttpService : IHttpService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpService> _logger;
    private readonly Dictionary<string, string> _customHeaders = new();
    private string? _bearerToken;

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public HttpService(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateHttpClient();
        
        try
        {
            _logger.LogDebug("发送 GET 请求: {Url}", url);
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("GET 请求成功: {Url}, 响应长度: {Length}", url, content.Length);
            
            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET 请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GET 请求超时或被取消: {Url}", url);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateHttpClient();
        
        try
        {
            _logger.LogDebug("发送 GET 请求并反序列化为 {Type}: {Url}", typeof(T).Name, url);
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("GET 请求成功: {Url}, 响应 JSON: {Json}", url, content);
            
            var result = JsonSerializer.Deserialize<T>(content, DefaultJsonOptions);
            _logger.LogDebug("反序列化成功: {Url}", url);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET 请求失败: {Url}", url);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "反序列化失败: {Url}, 类型: {Type}", url, typeof(T).Name);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GET 请求超时或被取消: {Url}", url);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> PostFormAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(formData);
        
        using var httpClient = CreateHttpClient();
        using var content = new FormUrlEncodedContent(formData);
        
        try
        {
            _logger.LogDebug("发送 POST 表单请求: {Url}, 参数数量: {Count}", url, formData.Count);
            
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("POST 表单请求成功: {Url}, 响应长度: {Length}", url, result.Length);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST 表单请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "POST 表单请求超时或被取消: {Url}", url);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> PostJsonAsync(string url, string jsonContent, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);
        
        using var httpClient = CreateHttpClient();
        using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        try
        {
            _logger.LogDebug("发送 POST JSON 请求: {Url}, 内容: {Json}", url, jsonContent);
            
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("POST JSON 请求成功: {Url}, 响应: {Json}", url, result);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST JSON 请求失败: {Url}", url);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "POST JSON 请求超时或被取消: {Url}", url);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        using var httpClient = CreateHttpClient();
        
        try
        {
            var requestJson = JsonSerializer.Serialize(request, DefaultJsonOptions);
            _logger.LogDebug("发送 POST JSON 请求: {Url}, 请求类型: {RequestType}, 响应类型: {ResponseType}, 请求 JSON: {Json}", 
                url, typeof(TRequest).Name, typeof(TResponse).Name, requestJson);
            
            var response = await httpClient.PostAsJsonAsync(url, request, DefaultJsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("POST JSON 请求成功: {Url}, 响应 JSON: {Json}", url, responseContent);
            
            var result = JsonSerializer.Deserialize<TResponse>(responseContent, DefaultJsonOptions);
            _logger.LogDebug("反序列化成功: {Url}", url);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST JSON 请求失败: {Url}", url);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "序列化或反序列化失败: {Url}, 请求类型: {RequestType}, 响应类型: {ResponseType}", 
                url, typeof(TRequest).Name, typeof(TResponse).Name);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "POST JSON 请求超时或被取消: {Url}", url);
            throw;
        }
    }

    /// <inheritdoc/>
    public void SetBearerToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        _bearerToken = token;
        _logger.LogDebug("设置 Bearer Token");
    }

    /// <inheritdoc/>
    public void AddHeader(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        _customHeaders[name] = value;
        _logger.LogDebug("添加自定义请求头: {Name} = {Value}", name, value);
    }

    /// <inheritdoc/>
    public void ClearHeaders()
    {
        _customHeaders.Clear();
        _logger.LogDebug("已清除所有自定义请求头");
    }

    /// <inheritdoc/>
    public void RemoveHeader(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        if (_customHeaders.Remove(name))
        {
            _logger.LogDebug("已移除自定义请求头: {Name}", name);
        }
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        // 禁用代理以避免本地调试问题
        // httpClient.DefaultRequestProxy = null; // 需要配置 HttpClientHandler
        
        // 设置 Bearer Token
        if (!string.IsNullOrWhiteSpace(_bearerToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
        }
        
        // 添加自定义请求头
        foreach (var header in _customHeaders)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        return httpClient;
    }
}
