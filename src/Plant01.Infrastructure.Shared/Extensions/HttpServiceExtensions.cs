using Microsoft.Extensions.DependencyInjection;
using Plant01.Domain.Shared.Interfaces;
using Plant01.Infrastructure.Shared.Services;

namespace Plant01.Infrastructure.Shared.Extensions;

/// <summary>
/// HttpService 服务注册扩展
/// </summary>
public static class HttpServiceExtensions
{
    /// <summary>
    /// 添加 HttpService 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureClient">配置 HttpClient 的委托（可选）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpService(
        this IServiceCollection services,
        Action<IHttpClientBuilder>? configureClient = null)
    {
        var builder = services.AddHttpClient<IHttpService, HttpService>()
            .ConfigureHttpClient(client =>
            {
                // 默认配置
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.Add("User-Agent", "Plant01.HttpService/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            });

        // 应用自定义配置
        configureClient?.Invoke(builder);

        // 注册服务
        services.AddScoped<IHttpService, HttpService>();

        return services;
    }

    /// <summary>
    /// 添加 HttpService 服务，带有命名的 HttpClient
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="clientName">HttpClient 名称</param>
    /// <param name="configureClient">配置 HttpClient 的委托（可选）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpService(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null)
    {
        services.AddHttpClient(clientName, client =>
        {
            // 默认配置
            client.Timeout = TimeSpan.FromMinutes(1);
            client.DefaultRequestHeaders.Add("User-Agent", "Plant01.HttpService/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            
            // 应用自定义配置
            configureClient?.Invoke(client);
        });

        // 注册服务
        services.AddScoped<IHttpService, HttpService>();

        return services;
    }
}
