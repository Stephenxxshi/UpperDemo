# MES 和 HttpService 文档索引

## ?? 文档导航

### ?? 快速开始（推荐从这里开始）

1. **[MES 锐派接口快速开始](MesService-QuickStart.md)**
   - ?? 5分钟快速开始
   - ?? 最常用的代码示例
   - ? 立即可用的代码片段

2. **[HttpService 快速参考](HttpService-QuickReference.md)**
   - ?? API 速查表
   - ?? 常用操作示例
   - ? 快速解决问题

### ?? 详细指南

3. **[MES 锐派接口详细指南](MesService-Revopac-Guide.md)**
   - ?? 认证机制详解
   - ?? 完整的请求/响应示例
   - ?? 故障排除指南
   - ?? 单元测试示例
   - ?? 最佳实践

4. **[HttpService 使用指南](HttpService-Usage.md)**
   - ??? 架构设计说明
   - ?? 现代化技术特性
   - ?? 高级配置选项
   - ?? 迁移指南
   - ?? 性能优化建议

### ?? 实现总结

5. **[MES 服务实现总结](MesService-Implementation-Summary.md)**
   - ? 完成的工作清单
   - ?? 认证实现细节
   - ?? 技术亮点
   - ?? 代码质量分析
   - ?? 扩展建议

6. **[HttpService 实现总结](HttpService-Implementation-Summary.md)**
   - ? 已创建的文件
   - ??? 架构建议
   - ?? 与旧代码对比
   - ?? 性能对比
   - ?? 下一步建议

---

## ??? 按使用场景导航

### 场景 1: 我是新手，第一次使用
1. 阅读 → **[MES 锐派接口快速开始](MesService-QuickStart.md)**
2. 参考 → **[HttpService 快速参考](HttpService-QuickReference.md)**
3. 查看示例代码 → `src/Plant01.Upper.Presentation.Core/ViewModels/MesRevopacViewModel.cs`

### 场景 2: 我需要集成 MES 接口
1. 配置 → 打开 `appsettings.json`，添加 MES 配置
2. 注入 → 在构造函数中注入 `IMesService`
3. 调用 → 参考 **[MES 锐派接口快速开始](MesService-QuickStart.md)** 中的代码示例

### 场景 3: 我需要调用其他 HTTP 接口
1. 学习 → **[HttpService 使用指南](HttpService-Usage.md)**
2. 参考 → **[HttpService 快速参考](HttpService-QuickReference.md)**
3. 实现 → 参考 `src/Plant01.Upper.Application/Services/MesService.cs`

### 场景 4: 我遇到了问题
1. 查看 → **[MES 锐派接口详细指南](MesService-Revopac-Guide.md)** 中的"故障排除"部分
2. 检查 → **[HttpService 快速参考](HttpService-QuickReference.md)** 中的"故障排除"部分
3. 查看日志 → 使用 Debug 日志级别查看详细信息

### 场景 5: 我想了解实现细节
1. 阅读 → **[MES 服务实现总结](MesService-Implementation-Summary.md)**
2. 阅读 → **[HttpService 实现总结](HttpService-Implementation-Summary.md)**
3. 查看源码 → `src/Plant01.Infrastructure.Shared/Services/HttpService.cs`

### 场景 6: 我需要添加新的 MES 接口
1. 参考 → **[MES 服务实现总结](MesService-Implementation-Summary.md)** 中的"后续扩展建议"
2. 模仿 → `src/Plant01.Upper.Application/Services/MesService.cs` 中的现有实现
3. 测试 → 参考 **[MES 锐派接口详细指南](MesService-Revopac-Guide.md)** 中的测试示例

---

## ?? 文件清单

### 核心代码文件

| 文件 | 说明 | 位置 |
|------|------|------|
| `IHttpService.cs` | HTTP 服务接口 | `src/Plant01.Domain.Shared/Interfaces/` |
| `HttpService.cs` | HTTP 服务实现 | `src/Plant01.Infrastructure.Shared/Services/` |
| `HttpServiceExtensions.cs` | DI 注册扩展 | `src/Plant01.Infrastructure.Shared/Extensions/` |
| `IMesService.cs` | MES 服务接口 | `src/Plant01.Upper.Application/Interfaces/` |
| `MesService.cs` | MES 服务实现 | `src/Plant01.Upper.Application/Services/` |
| `Bootstrapper.cs` | 服务注册 | `src/Plant01.Upper.Presentation.Bootstrapper/` |

### 示例代码文件

| 文件 | 说明 | 位置 |
|------|------|------|
| `MesWebApi.cs` | HttpService 使用示例 | `src/Plant01.Upper.Application/Services/` |
| `MesRevopacViewModel.cs` | MES 接口使用示例 | `src/Plant01.Upper.Presentation.Core/ViewModels/` |
| `MesLoginExampleViewModel.cs` | 登录接口示例 | `src/Plant01.Upper.Presentation.Core/ViewModels/` |

### 文档文件

| 文档 | 说明 | 位置 |
|------|------|------|
| `MesService-QuickStart.md` | MES 快速开始 | `docs/` |
| `MesService-Revopac-Guide.md` | MES 详细指南 | `docs/` |
| `MesService-Implementation-Summary.md` | MES 实现总结 | `docs/` |
| `HttpService-QuickReference.md` | HttpService 快速参考 | `docs/` |
| `HttpService-Usage.md` | HttpService 使用指南 | `docs/` |
| `HttpService-Implementation-Summary.md` | HttpService 实现总结 | `docs/` |
| `README-Index.md` | 本文档（索引） | `docs/` |

---

## ?? 核心概念速查

### IHttpService - HTTP 通信服务

```csharp
// 注入
public MyService(IHttpService httpService) { }

// GET 请求
var data = await _httpService.GetAsync<T>(url);

// POST JSON
var response = await _httpService.PostJsonAsync<TReq, TRes>(url, request);

// 认证
_httpService.SetBearerToken(token);
_httpService.AddHeader("X-Key", "value");
```

### IMesService - MES 接口服务

```csharp
// 注入
public MyService(IMesService mesService) { }

// 码垛完成
var response = await _mesService.FinishPalletizingAsync(request);

// 托盘缺少
var response = await _mesService.ReportLackPalletAsync(request);

// 判断成功
if (response.IsSuccess) { /* 成功 */ }
```

### 配置

```json
{
  "MesApi": {
    "BaseUrl": "http://WebAPIServer",
    "CorpNo": "020",
    "CorpId": "IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ"
  }
}
```

---

## ? 常见问题快速链接

| 问题 | 查看文档 |
|------|---------|
| 如何开始使用 MES 接口？ | [MES 快速开始](MesService-QuickStart.md) |
| 认证密钥如何生成？ | [MES 详细指南 - 认证机制](MesService-Revopac-Guide.md#-认证机制) |
| 如何调用其他 HTTP 接口？ | [HttpService 使用指南](HttpService-Usage.md) |
| 请求超时怎么办？ | [HttpService 快速参考 - 故障排除](HttpService-QuickReference.md#-故障排除) |
| 如何处理错误？ | [MES 快速开始 - 常见问题](MesService-QuickStart.md#-常见问题) |
| IHttpService 接口应该放在哪里？ | [HttpService 实现总结 - 架构建议](HttpService-Implementation-Summary.md#-关于接口位置的建议) |
| 如何进行单元测试？ | [MES 详细指南 - 单元测试](MesService-Revopac-Guide.md#-单元测试) |
| 如何添加新的 MES 接口？ | [MES 实现总结 - 扩展建议](MesService-Implementation-Summary.md#-后续扩展建议) |

---

## ?? 快速跳转

- ?? **立即开始** → [MES 锐派接口快速开始](MesService-QuickStart.md)
- ?? **深入学习** → [MES 锐派接口详细指南](MesService-Revopac-Guide.md)
- ?? **API 参考** → [HttpService 快速参考](HttpService-QuickReference.md)
- ?? **最佳实践** → [HttpService 使用指南](HttpService-Usage.md)
- ?? **实现细节** → [MES 服务实现总结](MesService-Implementation-Summary.md)

---

## ?? 需要帮助？

1. 查看本文档索引，找到对应的文档
2. 查看示例代码 (`src/Plant01.Upper.Presentation.Core/ViewModels/`)
3. 查看日志输出（启用 Debug 级别）
4. 查看单元测试示例

---

**?? 祝您使用愉快！** 如有问题，请参考上述文档。
