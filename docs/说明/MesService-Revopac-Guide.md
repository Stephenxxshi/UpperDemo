# MES 锐派接口使用指南

## 概述

`MesService` 实现了与 MES 系统的锐派（REVOPAC）接口对接，使用 `IHttpService` 进行 HTTP 通信，自动处理认证密钥生成。

## ?? 已实现的接口

### 1. 锐派码垛完成 (REVOPACFinishPalletizing)
- **接口地址**: `/api/cmsDeviceData/REVOPACFinishPalletizing`
- **请求方式**: POST
- **用途**: 上报码垛机完成码垛的数据

### 2. 锐派托盘缺少 (REVOPACLackPallet)
- **接口地址**: `/api/cmsDeviceData/REVOPACLackPallet`
- **请求方式**: POST
- **用途**: 上报托盘缺少情况

## ?? 认证机制

### 密钥生成规则

根据客户端要求，每次请求需要在 `Authorization` 请求头中传递认证密钥：

**密钥格式**: `CorpNo&auth_sys_time&auth_sign_code`

- **CorpNo**: 客户 Corpid 序号（如：020）
- **auth_sys_time**: 当前时间戳（10位数字，精确到秒）
- **auth_sign_code**: MD5 加密 `auth_sys_time&Corpid`

**有效期**: 2 分钟（根据 auth_sys_time 时间戳判断）

### 密钥生成示例

```csharp
// 假设配置如下：
CorpNo = "020"
CorpId = "IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ"

// 1. 获取当前时间戳
auth_sys_time = 1704067200  // 2024-01-01 00:00:00 UTC

// 2. 生成签名字符串
signString = "1704067200&IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ"

// 3. MD5 加密
auth_sign_code = MD5(signString)  // 例如: "a1b2c3d4e5f6..."

// 4. 组合最终密钥
authKey = "020&1704067200&a1b2c3d4e5f6..."
```

**注意**: 
- ? `MesService` 会自动为每次请求生成新的密钥
- ? 时间戳使用 UTC 时间
- ? MD5 哈希值为 32 位小写字符串

## ?? 配置

### appsettings.json

```json
{
  "MesApi": {
    "BaseUrl": "http://WebAPIServer",
    "CorpNo": "020",
    "CorpId": "IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ",
    "Timeout": "00:01:00",
    "RetryCount": 3
  }
}
```

**配置说明**:
- `BaseUrl`: MES API 服务器地址
- `CorpNo`: 客户序号
- `CorpId`: 客户密钥
- `Timeout`: 请求超时时间
- `RetryCount`: 重试次数（可选）

## ?? 使用示例

### 1. 依赖注入

```csharp
public class MyService
{
    private readonly IMesService _mesService;
    private readonly ILogger<MyService> _logger;

    public MyService(IMesService mesService, ILogger<MyService> logger)
    {
        _mesService = mesService;
        _logger = logger;
    }
}
```

### 2. 调用码垛完成接口

```csharp
public async Task ReportPalletizingCompleteAsync()
{
    try
    {
        var request = new FinishPalletizingRequest
        {
            AgvDeviceCode = "AGV001",
            PalletId = "P00001",
            DeviceCode = "Palletizing",
            JobId = 10000,
            List = new List<PackageDetail>
            {
                new() { BagNums = "A001", Quan = 20 },
                new() { BagNums = "A002", Quan = 30 }
            }
        };

        var response = await _mesService.FinishPalletizingAsync(request);

        if (response.IsSuccess)
        {
            _logger.LogInformation("码垛完成上报成功");
            // 处理成功逻辑
        }
        else
        {
            _logger.LogWarning("码垛完成上报失败: [{ErrorCode}] {ErrorMsg}", 
                response.ErrorCode, response.ErrorMsg);
            // 处理失败逻辑
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "码垛完成上报异常");
        // 处理异常
    }
}
```

### 3. 调用托盘缺少接口

```csharp
public async Task ReportPalletShortageAsync(string agvCode, int palletType)
{
    try
    {
        var request = new LackPalletRequest
        {
            AgvDeviceCode = agvCode,
            PalletType = palletType  // 1=母托盘, 2=子托盘
        };

        var response = await _mesService.ReportLackPalletAsync(request);

        if (response.IsSuccess)
        {
            _logger.LogInformation("托盘缺少上报成功");
        }
        else
        {
            _logger.LogWarning("托盘缺少上报失败: {ErrorMsg}", response.ErrorMsg);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "托盘缺少上报异常");
    }
}
```

### 4. 在 ViewModel 中使用

```csharp
public class ProductionViewModel
{
    private readonly IMesService _mesService;

    public ProductionViewModel(IMesService mesService)
    {
        _mesService = mesService;
    }

    private async Task OnPalletizingComplete()
    {
        var request = new FinishPalletizingRequest
        {
            AgvDeviceCode = CurrentAgvCode,
            PalletId = CurrentPalletId,
            DeviceCode = CurrentDeviceCode,
            JobId = CurrentJobId,
            List = PackageDetailList  // 从界面收集的数据
        };

        var result = await _mesService.FinishPalletizingAsync(request);
        
        if (result.IsSuccess)
        {
            StatusMessage = "码垛完成上报成功！";
        }
        else
        {
            StatusMessage = $"上报失败：{result.ErrorMsg}";
        }
    }
}
```

## ?? 请求和响应示例

### 锐派码垛完成

**请求体**:
```json
{
  "agvDeviceCode": "AGV001",
  "palletId": "P00001",
  "deviceCode": "Palletizing",
  "jobId": 10000,
  "list": [
    {
      "bagNums": "A001",
      "quan": 20
    },
    {
      "bagNums": "A002",
      "quan": 30
    }
  ]
}
```

**请求头**:
```http
Authorization: 020&1704067200&a1b2c3d4e5f6...
Accept: application/json
Content-Type: application/json
```

**响应体**:
```json
{
  "errorCode": 0,
  "errorMsg": ""
}
```

### 锐派托盘缺少

**请求体**:
```json
{
  "agvDeviceCode": "AGV001",
  "palletType": 1
}
```

**请求头**:
```http
Authorization: 020&1704067200&a1b2c3d4e5f6...
Accept: application/json
Content-Type: application/json
```

**响应体**:
```json
{
  "errorCode": 0,
  "errorMsg": ""
}
```

## ?? 错误处理

### 响应错误码

| 错误码 | 说明 | 处理建议 |
|--------|------|----------|
| 0 | 成功 | - |
| -1 | 响应为空 | 检查网络连接 |
| 其他 | 业务错误 | 根据 errorMsg 处理 |

### 异常处理

```csharp
try
{
    var response = await _mesService.FinishPalletizingAsync(request);
}
catch (HttpRequestException ex)
{
    // HTTP 请求异常（网络错误、服务器错误等）
    _logger.LogError(ex, "HTTP 请求失败");
}
catch (TaskCanceledException ex)
{
    // 请求超时或被取消
    _logger.LogWarning(ex, "请求超时");
}
catch (Exception ex)
{
    // 其他异常
    _logger.LogError(ex, "未知异常");
}
```

## ?? 故障排除

### 问题 1: 认证失败

**症状**: 返回认证相关错误

**原因**:
1. CorpNo 或 CorpId 配置错误
2. 时间戳超出有效期（2分钟）
3. MD5 签名计算错误

**解决**:
```csharp
// 检查配置
var config = _configuration.GetSection("MesApi");
_logger.LogDebug("CorpNo: {CorpNo}, CorpId: {CorpId}", 
    config["CorpNo"], config["CorpId"]);

// 检查时间戳
var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
_logger.LogDebug("Current timestamp: {Timestamp}", timestamp);
```

### 问题 2: 请求超时

**症状**: TaskCanceledException

**原因**: 网络延迟或服务器响应慢

**解决**:
```json
{
  "MesApi": {
    "Timeout": "00:02:00"  // 增加超时时间到 2 分钟
  }
}
```

### 问题 3: JSON 序列化错误

**症状**: 反序列化失败

**原因**: 字段名大小写不匹配

**解决**: 已处理，请求体使用小写驼峰命名（agvDeviceCode, palletId 等）

## ?? 最佳实践

### 1. 使用取消令牌

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var response = await _mesService.FinishPalletizingAsync(request, cts.Token);
```

### 2. 重试机制

```csharp
public async Task<MesApiResponse> FinishPalletizingWithRetryAsync(
    FinishPalletizingRequest request, 
    int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var response = await _mesService.FinishPalletizingAsync(request);
            if (response.IsSuccess)
            {
                return response;
            }

            _logger.LogWarning("尝试 {Attempt}/{MaxRetries} 失败", i + 1, maxRetries);
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // 指数退避
        }
        catch (Exception ex)
        {
            if (i == maxRetries - 1) throw;
            _logger.LogWarning(ex, "尝试 {Attempt}/{MaxRetries} 异常", i + 1, maxRetries);
        }
    }

    return new MesApiResponse { ErrorCode = -1, ErrorMsg = "重试次数已用尽" };
}
```

### 3. 批量处理

```csharp
public async Task<List<MesApiResponse>> FinishMultiplePalletizingAsync(
    List<FinishPalletizingRequest> requests)
{
    var tasks = requests.Select(req => _mesService.FinishPalletizingAsync(req));
    var responses = await Task.WhenAll(tasks);
    return responses.ToList();
}
```

### 4. 日志记录

```csharp
// 在关键节点记录日志
_logger.LogInformation("开始上报码垛完成，任务ID: {JobId}", request.JobId);
var response = await _mesService.FinishPalletizingAsync(request);
_logger.LogInformation("上报完成，结果: {IsSuccess}, 错误码: {ErrorCode}", 
    response.IsSuccess, response.ErrorCode);
```

## ?? 单元测试

```csharp
public class MesServiceTests
{
    [Fact]
    public async Task FinishPalletizingAsync_ShouldReturnSuccess_WhenRequestValid()
    {
        // Arrange
        var mockHttpService = new Mock<IHttpService>();
        mockHttpService
            .Setup(x => x.PostJsonAsync<object, MesApiResponse>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MesApiResponse { ErrorCode = 0, ErrorMsg = "" });

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["MesApi:BaseUrl"]).Returns("http://test");
        mockConfig.Setup(x => x["MesApi:CorpNo"]).Returns("020");
        mockConfig.Setup(x => x["MesApi:CorpId"]).Returns("test-corp-id");

        var service = new MesService(
            mockHttpService.Object,
            Mock.Of<ILogger<MesService>>(),
            mockConfig.Object);

        var request = new FinishPalletizingRequest
        {
            AgvDeviceCode = "AGV001",
            PalletId = "P00001",
            DeviceCode = "Palletizing",
            JobId = 10000,
            List = new List<PackageDetail>()
        };

        // Act
        var response = await service.FinishPalletizingAsync(request);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(0, response.ErrorCode);
    }
}
```

## ?? 关键技术点

### 1. 自动认证密钥生成
- ? 每次请求自动生成新密钥
- ? 使用 UTC 时间戳确保一致性
- ? MD5 哈希值为小写 32 位字符串

### 2. JSON 命名规范
- ? 请求体使用小写驼峰命名（agvDeviceCode）
- ? C# 模型使用 PascalCase（AgvDeviceCode）
- ? System.Text.Json 自动处理转换

### 3. 异常处理
- ? HTTP 请求异常
- ? 超时和取消
- ? JSON 序列化异常
- ? 业务错误码

### 4. 日志记录
- ? Debug: 认证密钥生成详情
- ? Info: 接口调用成功
- ? Warning: 业务错误
- ? Error: 异常情况

## ?? 相关文档

- **HttpService 使用指南**: `docs/HttpService-Usage.md`
- **HttpService 快速参考**: `docs/HttpService-QuickReference.md`
- **示例代码**: `src/Plant01.Upper.Presentation.Core/ViewModels/MesRevopacViewModel.cs`

---

**?? 开始使用**: 只需注入 `IMesService` 即可调用锐派接口！
