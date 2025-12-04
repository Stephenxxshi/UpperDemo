namespace Plant01.Domain.Shared.Interfaces;

/// <summary>
/// HTTP 服务接口
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// 发送 GET 请求
    /// </summary>
    Task<string> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 GET 请求并反序列化为指定类型
    /// </summary>
    Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 POST 表单请求
    /// </summary>
    Task<string> PostFormAsync(string url, Dictionary<string, string> formData, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 POST JSON 请求
    /// </summary>
    Task<string> PostJsonAsync(string url, string jsonContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 POST JSON 请求（强类型）
    /// </summary>
    Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置授权令牌
    /// </summary>
    void SetBearerToken(string token);

    /// <summary>
    /// 添加自定义请求头
    /// </summary>
    void AddHeader(string name, string value);

    /// <summary>
    /// 清除所有自定义请求头
    /// </summary>
    void ClearHeaders();

    /// <summary>
    /// 移除指定的自定义请求头
    /// </summary>
    void RemoveHeader(string name);
}
