# HttpService 使用指南

## 概述

`HttpService` 是一个基于 .NET 10 的现代化 HTTP 客户端服务实现，使用了以下最佳实践：

- ? 使用 `IHttpClientFactory` 管理 HttpClient 生命周期
- ? 支持依赖注入
- ? 完整的日志记录
- ? 取消令牌支持
- ? 强类型 JSON 序列化/反序列化
- ? 异常处理和重试（可扩展）

## 架构说明

### 为什么接口在 Domain.Shared？

**当前位置**: `Plant01.Domain.Shared.Interfaces.IHttpService`
**建议位置**: 应该移动到 `Plant01.Infrastructure.Shared`

**原因**：
- HTTP 服务是**基础设施关注点**，不属于领域层
- `Domain.Shared` 应该只包含纯领域概念（实体、值对象、领域事件等）
- 基础设施接口应该放在基础设施层

### 推荐的项目结构

```
Plant01.Domain.Shared/           → 领域共享（实体、值对象、领域事件）
Plant01.Infrastructure.Shared/   → 基础设施共享（IHttpService、日志、缓存等）
  ├── Services/
  │   └── HttpService.cs         → 实现类
  └── Extensions/
      └── HttpServiceExtensions.cs → 服务注册
```

## 使用方式

### 1. 基本注册（已在 Bootstrapper 中完成）

```csharp
services.AddHttpService();
```

### 2. 高级配置

```csharp
// 自定义超时和请求头
services.AddHttpService(builder =>
{
    builder.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri("https://api.example.com");
    });
    
    // 添加 Polly 弹性策略（需要安装 Microsoft.Extensions.Http.Resilience）
    builder.AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
    });
});
```

### 3. 在服务中使用

```csharp
public class MesWebApiService
{
    private readonly IHttpService _httpService;
    private readonly ILogger<MesWebApiService> _logger;

    public MesWebApiService(IHttpService httpService, ILogger<MesWebApiService> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }

    public async Task<ProductInfo?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 方式1: 直接返回强类型
            var product = await _httpService.GetAsync<ProductInfo>(
                $"https://api.mes.com/products/{productId}", 
                cancellationToken);
            
            return product;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "获取产品信息失败");
            throw;
        }
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var loginRequest = new LoginRequest 
        { 
            Username = username, 
            Password = password 
        };

        // 方式2: 强类型请求和响应
        var response = await _httpService.PostJsonAsync<LoginRequest, LoginResponse>(
            "https://api.mes.com/auth/login",
            loginRequest);

        if (response?.Token != null)
        {
            // 设置后续请求的 Bearer Token
            _httpService.SetBearerToken(response.Token);
        }

        return response?.Token ?? string.Empty;
    }

    public async Task<string> UploadFormDataAsync()
    {
        var formData = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // 方式3: 发送表单数据
        return await _httpService.PostFormAsync(
            "https://api.mes.com/upload",
            formData);
    }
}
```

### 4. 添加自定义请求头

```csharp
// 添加自定义请求头
_httpService.AddHeader("X-Custom-Header", "CustomValue");
_httpService.AddHeader("X-Request-Id", Guid.NewGuid().ToString());

// 后续所有请求都会包含这些请求头
var result = await _httpService.GetAsync("https://api.example.com/data");
```

## 现代化特性

### 1. ArgumentException.ThrowIfNull（.NET 6+）
```csharp
ArgumentNullException.ThrowIfNull(request);
ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);
```

### 2. 使用 HttpClient 扩展方法
```csharp
// 现代化方式
await httpClient.GetFromJsonAsync<T>(url);
await httpClient.PostAsJsonAsync(url, request);

// 而不是旧方式
var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");
await httpClient.PostAsync(url, content);
```

### 3. IHttpClientFactory 的优势

- ? 自动管理 HttpClient 生命周期，避免端口耗尽
- ? 支持命名和类型化客户端
- ? 内置连接池管理
- ? 可配置的重试和断路器策略
- ? DNS 刷新机制

### 4. 可选的弹性策略（Polly）

安装包：
```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

配置：
```csharp
services.AddHttpService(builder =>
{
    builder.AddStandardResilienceHandler(options =>
    {
        // 重试策略
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        
        // 断路器
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        
        // 超时
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    });
});
```

## 性能优化建议

1. **使用 ValueTask（高性能场景）**: 如果需要更高性能，可以将返回类型改为 `ValueTask<T>`
2. **启用 HTTP/2**: 在 HttpClientHandler 中启用 HTTP/2
3. **连接池调优**: 通过 SocketsHttpHandler 调整连接池参数
4. **使用 System.Text.Json**: 已使用，比 Newtonsoft.Json 性能更好

## 测试

```csharp
public class HttpServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnData()
    {
        // Arrange
        var mockFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<HttpService>>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When("https://api.test.com/data")
            .Respond("application/json", "{\"id\":1,\"name\":\"Test\"}");
        
        var httpClient = mockHttpMessageHandler.ToHttpClient();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        var service = new HttpService(mockFactory.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetAsync<TestData>("https://api.test.com/data");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }
}
```

## 迁移指南

如果你当前使用 `HttpClientUtility.DefaultClient`，可以这样迁移：

### 旧代码
```csharp
var client = HttpClientUtility.DefaultClient;
var response = await client.GetAsync(url);
var content = await response.Content.ReadAsStringAsync();
```

### 新代码
```csharp
// 通过依赖注入获取
public MyService(IHttpService httpService)
{
    _httpService = httpService;
}

// 使用
var content = await _httpService.GetAsync(url);
// 或
var data = await _httpService.GetAsync<MyDataType>(url);
```

## 最佳实践

1. ? 总是传递 `CancellationToken`
2. ? 使用强类型而非字符串
3. ? 在服务层注入 `IHttpService`，而非在 ViewModel 中
4. ? 使用适当的日志级别
5. ? 处理超时和取消异常
6. ? 不要在每个请求中创建新的 HttpClient
7. ? 不要忽略异常

## 相关资源

- [IHttpClientFactory 最佳实践](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/httpclient-factory)
- [Polly 弹性策略](https://www.pollydocs.org/)
- [System.Text.Json 文档](https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json/overview)
