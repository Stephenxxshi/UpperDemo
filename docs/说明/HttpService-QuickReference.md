# HttpService 快速参考

## ?? 已创建的文件

```
? src\Plant01.Infrastructure.Shared\Services\HttpService.cs
? src\Plant01.Infrastructure.Shared\Extensions\HttpServiceExtensions.cs
? src\Plant01.Upper.Application\Services\MesWebApi.cs
? src\Plant01.Upper.Application\Interfaces\IMesWebApi.cs
? src\Plant01.Upper.Presentation.Core\ViewModels\MesLoginExampleViewModel.cs
? docs\HttpService-Usage.md
? docs\HttpService-Implementation-Summary.md
```

## ?? 常用操作

### GET 请求
```csharp
// 返回字符串
var html = await _httpService.GetAsync("https://api.example.com");

// 返回强类型
var user = await _httpService.GetAsync<User>("https://api.example.com/user/123");
```

### POST 表单
```csharp
var formData = new Dictionary<string, string>
{
    ["username"] = "admin",
    ["password"] = "123456"
};
var result = await _httpService.PostFormAsync(url, formData);
```

### POST JSON (字符串)
```csharp
var json = """{"name":"Product","price":99.99}""";
var result = await _httpService.PostJsonAsync(url, json);
```

### POST JSON (强类型)
```csharp
var request = new CreateProductRequest { Name = "Product", Price = 99.99m };
var response = await _httpService.PostJsonAsync<CreateProductRequest, CreateProductResponse>(
    url, 
    request,
    cancellationToken);
```

### 设置认证
```csharp
// Bearer Token
_httpService.SetBearerToken("your-jwt-token");

// 自定义请求头
_httpService.AddHeader("X-API-Key", "your-api-key");
_httpService.AddHeader("X-Request-Id", Guid.NewGuid().ToString());
```

## ?? API 一览表

| 方法 | 用途 | 示例 |
|------|------|------|
| `GetAsync(url)` | GET 返回字符串 | `await _httpService.GetAsync(url)` |
| `GetAsync<T>(url)` | GET 返回对象 | `await _httpService.GetAsync<User>(url)` |
| `PostFormAsync(url, form)` | POST 表单 | `await _httpService.PostFormAsync(url, dict)` |
| `PostJsonAsync(url, json)` | POST JSON 字符串 | `await _httpService.PostJsonAsync(url, json)` |
| `PostJsonAsync<TReq, TRes>(url, req)` | POST JSON 对象 | `await _httpService.PostJsonAsync<Req, Res>(url, req)` |
| `SetBearerToken(token)` | 设置 Bearer Token | `_httpService.SetBearerToken(token)` |
| `AddHeader(name, value)` | 添加自定义请求头 | `_httpService.AddHeader("X-Key", "value")` |

## ?? 配置选项

### appsettings.json
```json
{
  "MesApi": {
    "BaseUrl": "https://mes.example.com",
    "Timeout": "00:01:00",
    "RetryCount": 3
  }
}
```

### 服务注册
```csharp
// Bootstrapper.cs
services.AddHttpService(builder =>
{
    builder.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri("https://api.example.com");
    });
});
```

## ??? 架构建议

### 当前
```
Domain.Shared/
  └── Interfaces/
      └── IHttpService.cs  ? 不建议（但可用）
```

### 推荐
```
Infrastructure.Shared/
  └── Interfaces/
      └── IHttpService.cs  ? 推荐
```

## ?? 故障排除

### 问题: 请求超时
```csharp
// 解决: 增加超时时间或添加取消令牌
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var result = await _httpService.GetAsync(url, cts.Token);
```

### 问题: SSL 证书错误
```csharp
// 解决: 在服务注册时配置 HttpHandler
services.AddHttpService(builder =>
{
    builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });
});
```

### 问题: JSON 反序列化失败
```csharp
// 原因: 属性名不匹配或类型不兼容
// 解决: 使用 JsonPropertyName 特性
public class User
{
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }
}
```

## ?? 日志级别

| 级别 | 场景 | 示例 |
|------|------|------|
| Debug | 请求详情 | "发送 GET 请求: {Url}" |
| Info | 成功操作 | "登录成功，用户: {Username}" |
| Warning | 可恢复错误 | "请求超时或被取消: {Url}" |
| Error | 严重错误 | "POST JSON 请求失败: {Url}" |

## ?? 测试示例

```csharp
[Fact]
public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
{
    // Arrange
    var mockHttpService = new Mock<IHttpService>();
    mockHttpService
        .Setup(x => x.PostJsonAsync<LoginRequest, LoginResponse>(
            It.IsAny<string>(), 
            It.IsAny<LoginRequest>(), 
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new LoginResponse { Success = true, Token = "test-token" });
    
    var mesApi = new MesWebApi(mockHttpService.Object, Mock.Of<ILogger<MesWebApi>>(), Mock.Of<IConfiguration>());
    
    // Act
    var token = await mesApi.LoginAsync("user", "pass");
    
    // Assert
    Assert.Equal("test-token", token);
}
```

## ?? 相关文档

- **详细使用指南**: `docs/HttpService-Usage.md`
- **实现总结**: `docs/HttpService-Implementation-Summary.md`
- **示例代码**: `src/Plant01.Upper.Application/Services/MesWebApi.cs`
- **ViewModel 示例**: `src/Plant01.Upper.Presentation.Core/ViewModels/MesLoginExampleViewModel.cs`

## ? .NET 10 现代特性

- ? `IHttpClientFactory` - 连接池管理
- ? `HttpClient` 扩展方法 (`GetFromJsonAsync`, `PostAsJsonAsync`)
- ? `System.Text.Json` - 高性能序列化
- ? `ArgumentNullException.ThrowIfNull` - 参数验证
- ? Record types - 不可变数据模型
- ? Nullable reference types - 空引用安全
- ? Top-level statements & file-scoped namespaces

## ?? 性能提示

1. **重用 IHttpService 实例** - 通过 DI 单例注入
2. **使用 CancellationToken** - 支持取消和超时
3. **避免同步阻塞** - 始终使用 `await`
4. **合理设置超时** - 避免无限等待
5. **启用 HTTP/2** - 多路复用提升性能

---

**?? 开始使用**: 只需注入 `IHttpService` 即可开始发起 HTTP 请求！
