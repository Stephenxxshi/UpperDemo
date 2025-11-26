using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text;

namespace Plant01.Core.Utilities;

/// <summary>
/// HTTP 工具类，提供常用的 HTTP 请求方法
/// </summary>
public static class HttpUtility
{
    private static readonly Lazy<HttpClient> _defaultClient = new Lazy<HttpClient>(CreateDefaultHttpClient);

    /// <summary>
    /// 获取默认的 HttpClient 实例（单例模式）
    /// </summary>
    public static HttpClient DefaultClient => _defaultClient.Value;

    /// <summary>
    /// 创建默认的 HttpClient 实例
    /// </summary>
    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false,
            UseProxy = false,
            MaxConnectionsPerServer = 256,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                // 警告：仅在开发环境使用，生产环境应该移除此配置
                RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
#if DEBUG
                    return true; // 开发环境跳过证书验证
#else
                    return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None; // 生产环境验证证书
#endif
                }
            }
        };

        var httpClient = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromMinutes(1)
        };

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.ConnectionClose = true; // 替代 KeepAlive: false
        httpClient.DefaultRequestHeaders.ExpectContinue = false;

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

        return httpClient;
    }

    /// <summary>
    /// 创建自定义配置的 HttpClient 实例
    /// </summary>
    /// <param name="timeout">请求超时时间</param>
    /// <param name="validateCertificate">是否验证 SSL 证书</param>
    /// <returns>HttpClient 实例</returns>
    public static HttpClient CreateClient(TimeSpan? timeout = null, bool validateCertificate = true)
    {
        var handler = new SocketsHttpHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false,
            UseProxy = false,
            MaxConnectionsPerServer = 256,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                RemoteCertificateValidationCallback = validateCertificate
                    ? null
                    : delegate { return true; }
            }
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            Timeout = timeout ?? TimeSpan.FromMinutes(1)
        };
    }

    /// <summary>
    /// 设置授权令牌
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="token">令牌值</param>
    /// <param name="scheme">授权方案（默认为 Bearer）</param>
    public static void SetAuthorization(HttpClient httpClient, string token, string scheme = "Bearer")
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
    }

    /// <summary>
    /// 发送 GET 请求
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="url">请求地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应内容字符串</returns>
    public static async Task<string> GetAsync(HttpClient httpClient, string url, CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"GET request to '{url}' failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"GET request to '{url}' timed out", ex);
        }
    }

    /// <summary>
    /// 发送 GET 请求并反序列化为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="url">请求地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>反序列化后的对象</returns>
    public static async Task<T?> GetAsync<T>(HttpClient httpClient, string url, CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            return await httpClient.GetFromJsonAsync<T>(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"GET request to '{url}' failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"GET request to '{url}' timed out", ex);
        }
    }

    /// <summary>
    /// 发送 POST 表单请求
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="url">请求地址</param>
    /// <param name="parameter">表单参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应内容字符串</returns>
    public static async Task<string> PostFormAsync(HttpClient httpClient, string url, string parameter, CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            using var content = new StringContent(parameter ?? string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"POST form request to '{url}' failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"POST form request to '{url}' timed out", ex);
        }
    }

    /// <summary>
    /// 发送 POST JSON 请求
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="url">请求地址</param>
    /// <param name="parameter">JSON 参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应内容字符串</returns>
    public static async Task<string> PostJsonAsync(HttpClient httpClient, string url, string parameter, CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            using var content = new StringContent(parameter ?? string.Empty, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"POST JSON request to '{url}' failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"POST JSON request to '{url}' timed out", ex);
        }
    }

    /// <summary>
    /// 发送 POST JSON 请求并反序列化响应
    /// </summary>
    /// <typeparam name="TRequest">请求对象类型</typeparam>
    /// <typeparam name="TResponse">响应对象类型</typeparam>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="url">请求地址</param>
    /// <param name="request">请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>反序列化后的响应对象</returns>
    public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
        HttpClient httpClient,
        string url,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            var response = await httpClient.PostAsJsonAsync(url, request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"POST JSON request to '{url}' failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"POST JSON request to '{url}' timed out", ex);
        }
    }

    /// <summary>
    /// 添加自定义请求头
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="name">请求头名称</param>
    /// <param name="value">请求头值</param>
    public static void AddHeader(HttpClient httpClient, string name, string value)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Header name cannot be null or empty", nameof(name));

        httpClient.DefaultRequestHeaders.Remove(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            httpClient.DefaultRequestHeaders.Add(name, value);
        }
    }

    // 保留旧方法名作为兼容性别名
    [Obsolete("Use GetAsync instead")]
    public static Task<string> Get(HttpClient httpClient, string url)
        => GetAsync(httpClient, url);

    [Obsolete("Use PostJsonAsync instead")]
    public static Task<string> PostJSONAsync(HttpClient httpClient, string url, string parameter)
        => PostJsonAsync(httpClient, url, parameter);
}
