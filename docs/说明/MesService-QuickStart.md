# MES 锐派接口快速使用示例

## ?? 文件清单

```
? src\Plant01.Upper.Application\Interfaces\IMesService.cs  - 接口定义
? src\Plant01.Upper.Application\Services\MesService.cs     - 服务实现
? src\Plant01.Upper.Presentation.Core\ViewModels\MesRevopacViewModel.cs  - 使用示例
? docs\MesService-Revopac-Guide.md                          - 详细指南
```

## ?? 5分钟快速开始

### 1. 配置 appsettings.json

```json
{
  "MesApi": {
    "BaseUrl": "http://WebAPIServer",
    "CorpNo": "020",
    "CorpId": "IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ"
  }
}
```

### 2. 注入服务

```csharp
public class YourService
{
    private readonly IMesService _mesService;

    public YourService(IMesService mesService)
    {
        _mesService = mesService;
    }
}
```

### 3. 调用接口

#### 码垛完成

```csharp
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
    Console.WriteLine("上报成功！");
}
else
{
    Console.WriteLine($"上报失败：{response.ErrorMsg}");
}
```

#### 托盘缺少

```csharp
var request = new LackPalletRequest
{
    AgvDeviceCode = "AGV001",
    PalletType = 1  // 1=母托盘, 2=子托盘
};

var response = await _mesService.ReportLackPalletAsync(request);

if (response.IsSuccess)
{
    Console.WriteLine("上报成功！");
}
```

## ?? 认证说明

认证密钥 **自动生成**，无需手动处理！

**生成规则**:
```
密钥 = CorpNo & 时间戳 & MD5(时间戳&CorpId)
示例 = 020&1704067200&a1b2c3d4e5f6...
```

**有效期**: 2 分钟

## ?? 接口对照表

| 接口名称 | 方法 | 用途 |
|---------|------|------|
| `FinishPalletizingAsync` | POST | 上报码垛完成 |
| `ReportLackPalletAsync` | POST | 上报托盘缺少 |

## ?? 请求参数速查

### 码垛完成参数

| 参数 | 类型 | 必填 | 说明 |
|-----|------|------|------|
| AgvDeviceCode | string | ? | AGV 设备标识 |
| PalletId | string | ? | 共享托盘 ID |
| DeviceCode | string | ? | 码垛机设备标识 |
| JobId | int | ? | MES 生产任务 ID |
| List | List | ? | 打包明细列表 |
| ? BagNums | string | ? | 包号 |
| ? Quan | decimal | ? | 数量 |

### 托盘缺少参数

| 参数 | 类型 | 必填 | 说明 |
|-----|------|------|------|
| AgvDeviceCode | string | ? | AGV 设备标识 |
| PalletType | int | ? | 托盘类型（1=母托盘, 2=子托盘） |

## ?? 响应格式

```csharp
public record MesApiResponse
{
    public int ErrorCode { get; init; }      // 0=成功
    public string ErrorMsg { get; init; }    // 错误信息
    public bool IsSuccess => ErrorCode == 0; // 便捷属性
}
```

## ?? 常见问题

### Q: 如何判断调用成功？

```csharp
if (response.IsSuccess)  // 推荐：使用 IsSuccess 属性
{
    // 成功
}

// 或者
if (response.ErrorCode == 0)
{
    // 成功
}
```

### Q: 如何处理错误？

```csharp
var response = await _mesService.FinishPalletizingAsync(request);

if (!response.IsSuccess)
{
    _logger.LogWarning("调用失败：[{ErrorCode}] {ErrorMsg}", 
        response.ErrorCode, response.ErrorMsg);
    
    // 根据错误码处理...
}
```

### Q: 如何处理异常？

```csharp
try
{
    var response = await _mesService.FinishPalletizingAsync(request);
}
catch (HttpRequestException ex)
{
    // 网络错误
    _logger.LogError(ex, "网络请求失败");
}
catch (TaskCanceledException ex)
{
    // 超时
    _logger.LogWarning(ex, "请求超时");
}
```

## ?? 高级用法

### 使用取消令牌

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var response = await _mesService.FinishPalletizingAsync(request, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("操作已取消");
}
```

### 批量处理

```csharp
var tasks = requestList.Select(req => 
    _mesService.FinishPalletizingAsync(req));

var responses = await Task.WhenAll(tasks);

var successCount = responses.Count(r => r.IsSuccess);
Console.WriteLine($"成功: {successCount}/{responses.Length}");
```

## ?? 完整示例

```csharp
public class PalletizingService
{
    private readonly IMesService _mesService;
    private readonly ILogger<PalletizingService> _logger;

    public PalletizingService(IMesService mesService, ILogger<PalletizingService> logger)
    {
        _mesService = mesService;
        _logger = logger;
    }

    public async Task<bool> ReportPalletizingCompleteAsync(
        string agvCode, 
        string palletId, 
        int jobId, 
        List<(string bagNum, decimal quantity)> packages)
    {
        try
        {
            var request = new FinishPalletizingRequest
            {
                AgvDeviceCode = agvCode,
                PalletId = palletId,
                DeviceCode = "Palletizing",
                JobId = jobId,
                List = packages.Select(p => new PackageDetail
                {
                    BagNums = p.bagNum,
                    Quan = p.quantity
                }).ToList()
            };

            _logger.LogInformation("开始上报码垛完成，任务ID: {JobId}", jobId);

            var response = await _mesService.FinishPalletizingAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("码垛完成上报成功，任务ID: {JobId}", jobId);
                return true;
            }
            else
            {
                _logger.LogWarning("码垛完成上报失败，任务ID: {JobId}, 错误: [{ErrorCode}] {ErrorMsg}",
                    jobId, response.ErrorCode, response.ErrorMsg);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "码垛完成上报异常，任务ID: {JobId}", jobId);
            return false;
        }
    }

    public async Task<bool> ReportPalletShortageAsync(string agvCode, bool isParentPallet)
    {
        try
        {
            var request = new LackPalletRequest
            {
                AgvDeviceCode = agvCode,
                PalletType = isParentPallet ? 1 : 2
            };

            _logger.LogInformation("开始上报托盘缺少，AGV: {AgvCode}", agvCode);

            var response = await _mesService.ReportLackPalletAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("托盘缺少上报成功，AGV: {AgvCode}", agvCode);
                return true;
            }
            else
            {
                _logger.LogWarning("托盘缺少上报失败，AGV: {AgvCode}, 错误: {ErrorMsg}",
                    agvCode, response.ErrorMsg);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "托盘缺少上报异常，AGV: {AgvCode}", agvCode);
            return false;
        }
    }
}
```

## ?? 测试代码

```csharp
// 单元测试示例
[Fact]
public async Task FinishPalletizingAsync_ShouldReturnSuccess()
{
    // Arrange
    var mockMesService = new Mock<IMesService>();
    mockMesService
        .Setup(x => x.FinishPalletizingAsync(
            It.IsAny<FinishPalletizingRequest>(), 
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new MesApiResponse { ErrorCode = 0 });

    // Act
    var response = await mockMesService.Object.FinishPalletizingAsync(
        new FinishPalletizingRequest
        {
            AgvDeviceCode = "AGV001",
            PalletId = "P001",
            DeviceCode = "Palletizing",
            JobId = 1,
            List = new List<PackageDetail>()
        });

    // Assert
    Assert.True(response.IsSuccess);
}
```

## ?? 更多信息

- **详细指南**: `docs/MesService-Revopac-Guide.md`
- **HttpService 文档**: `docs/HttpService-Usage.md`
- **示例 ViewModel**: `src/Plant01.Upper.Presentation.Core/ViewModels/MesRevopacViewModel.cs`

---

**?? 就是这么简单！** 只需 3 步：配置 → 注入 → 调用
