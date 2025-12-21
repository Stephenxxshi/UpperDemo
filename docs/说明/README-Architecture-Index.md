# 工位-设备架构文档索引

## 📚 架构文档

### 🚀 快速开始

7. **[工位-设备架构快速入门](Workstation-Equipment-QuickStart.md)** ⭐ 推荐
   - ⏱️ 5分钟快速上手
   - 📝 完整工作流程示例
   - 💡 常见场景代码片段
   - ❓ 常见问题解答

8. **[工位-设备架构完整说明](Workstation-Equipment-Architecture.md)**
   - 📐 架构设计理念
   - 🔗 层次关系详解
   - 🛠️ 标签生成两种模式
   - 📂 文件结构索引
   - 🎯 扩展路线图

---

## 🗺️ 核心概念

### 业务层模型
- **Workstation (工位)**: 生产单元,可包含多个设备
- **Equipment (设备)**: 物理设备,具有类型和能力
- **EquipmentTagMapping**: 业务设备与通讯层标签的映射关系

### 通讯层模型
- **Channel (通道)**: PLC网络连接 (如 192.168.0.100:102)
- **Device (设备)**: PLC设备实例 (如 Siemens S7-1500)
- **Tag (标签)**: 数据点 (如 DB1.DBX0.0)

### 标签生成
- **Schema模式**: 从JSON定义的DB结构自动生成标签
- **Rules模式**: 按规则批量生成测试/模拟标签

---

## 📖 使用场景路线图

### 场景 1: 新增一个工位
1. 创建 Schema 文件 → 生成标签 → 创建工位实体 → 添加设备 → 建立映射
2. 参考: [快速入门 - 场景1](Workstation-Equipment-QuickStart.md#31-场景1-新增一个工位)

### 场景 2: 批量生成测试标签
1. 定义规则 → 预览 → 合并到配置文件
2. 参考: [快速入门 - 场景2](Workstation-Equipment-QuickStart.md#32-场景2-批量生成测试标签)

### 场景 3: 监控设备状态
1. 查询工位 → 遍历设备 → 读取标签映射 → 显示数据
2. 参考: [快速入门 - 第2.4节](Workstation-Equipment-QuickStart.md#24-使用通讯服务读写标签)

---

## 📁 核心文件清单

### 领域层

| 文件 | 说明 | 位置 |
|------|------|------|
| `Workstation.cs` | 工位实体 | `src/Plant01.Upper.Domain/Entities/` |
| `Equipment.cs` | 设备实体 | `src/Plant01.Upper.Domain/Entities/` |
| `EquipmentTagMapping.cs` | 标签映射 | `src/Plant01.Upper.Domain/Entities/` |
| `EquipmentType.cs` | 设备类型枚举 | `src/Plant01.Domain.Shared/Models/Equipment/` |
| `Capabilities.cs` | 设备能力枚举 | `src/Plant01.Domain.Shared/Models/Equipment/` |

### 应用层

| 文件 | 说明 | 位置 |
|------|------|------|
| `ITagGenerationService.cs` | 标签生成接口 | `src/Plant01.Upper.Application/Services/` |

### 基础设施层

| 文件 | 说明 | 位置 |
|------|------|------|
| `TagGenerationServiceImpl.cs` | 标签生成实现 | `src/Plant01.Upper.Infrastructure/Services/` |
| `ConfigurationLoader.cs` | 配置加载器 | `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/` |
| `S7AddressScanner.cs` | S7地址扫描器 | `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/` |
| `S7AddressParser.cs` | S7地址解析器 | `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/` |
| `SiemensS7Driver.cs` | 西门子S7驱动 | `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/` |

### 配置文件

| 文件 | 说明 | 位置 |
|------|------|------|
| `channels.csv` | 通道配置 | `src/Plant01.Upper.Infrastructure/Configs/` |
| `tags.csv` | 标签配置 (生成目标) | `src/Plant01.Upper.Infrastructure/Configs/` |
| `*.schema.json` | DB结构Schema | `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/` |
| `DB1.schema.json` | 示例Schema | `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/` |

### 文档

| 文档 | 说明 | 位置 |
|------|------|------|
| `Workstation-Equipment-QuickStart.md` | 快速入门 | `docs/` |
| `Workstation-Equipment-Architecture.md` | 完整架构文档 | `docs/` |
| `README-Architecture-Index.md` | 架构文档索引 (本文档) | `docs/` |

---

## 🛠️ API 快速参考

### ITagGenerationService - 标签生成服务

```csharp
// 注入
public MyService(ITagGenerationService tagService) { }

// 从Schema生成预览
var preview = await _tagService.PreviewTagsFromDbSchemaAsync(
    dbNumber: 1,
    backupFilePath: "tags_backup.csv"
);

// 从规则生成预览
var preview = await _tagService.PreviewTagsByRulesAsync(
    rulesObj: new AddressRules { DbNumber = 2, ... },
    backupFilePath: "tags_backup.csv"
);

// 合并到正式文件
var result = await _tagService.MergeGeneratedTagsAsync(
    preview,
    backupFilePath: "tags_backup.csv"
);
```

### S7AddressScanner - S7地址扫描

```csharp
var scanner = new S7AddressScanner();

// 测试连接
var isOk = await scanner.TestConnectionAsync(
    ipAddress: "192.168.0.100",
    port: 102,
    rack: 0,
    slot: 1
);

// 从Schema生成
var tags = await scanner.GenerateFromSchemaAsync(schema);

// 从规则生成
var tags = await scanner.GenerateByRulesAsync(rules);
```

### 实体操作

```csharp
// 创建工位
var ws = new Workstation
{
    Code = "WS-01",
    Name = "一号工位",
    Status = WorkstationStatus.Running,
    Enabled = true
};

// 创建设备
var eq = new Equipment
{
    Code = "BPK-01",
    Name = "袋料拾取机",
    EquipmentType = EquipmentType.BagPicker,
    Capabilities = Capabilities.Heartbeat | Capabilities.AlarmReport
};

// 添加映射
eq.AddTagMapping(new EquipmentTagMapping
{
    TagName = "WS01_Heartbeat",
    Purpose = EquipmentTagMapping.TagPurpose.Heartbeat,
    Direction = "R"
});

// 关联
ws.AddEquipment(eq);
```

---

## ❓ 常见问题速查

| 问题 | 查看文档 |
|------|---------|
| 如何快速开始? | [快速入门](Workstation-Equipment-QuickStart.md) |
| 标签生成两种模式有什么区别? | [完整架构 - 标签生成](Workstation-Equipment-Architecture.md#3-标签自动生成) |
| Schema文件怎么写? | [完整架构 - Schema示例](Workstation-Equipment-Architecture.md#示例db-schema文件) |
| 如何测试S7连接? | [快速入门 - Q&A](Workstation-Equipment-QuickStart.md#q1-如何测试s7连接) |
| 标签生成后如何回滚? | [快速入门 - Q&A](Workstation-Equipment-QuickStart.md#q2-标签生成后如何回滚) |
| 支持哪些数据类型? | [快速入门 - Q&A](Workstation-Equipment-QuickStart.md#q4-支持哪些s7数据类型) |
| 设备类型有哪些? | [完整架构 - EquipmentType](Workstation-Equipment-Architecture.md#equipmenttype) |
| 设备能力如何组合? | [完整架构 - Capabilities](Workstation-Equipment-Architecture.md#capabilities) |

---

## 🎯 下一步推荐

- 🚀 **立即开始** → [工位-设备架构快速入门](Workstation-Equipment-QuickStart.md)
- 📖 **深入学习** → [工位-设备架构完整说明](Workstation-Equipment-Architecture.md)
- 🔍 **查看示例** → `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB1.schema.json`
- 💻 **实现Repository** → 创建 WorkstationRepository, EquipmentRepository
- 🖥️ **UI集成** → 创建工位管理、设备监控界面

---

**💡 提示**: 建议先阅读 [快速入门](Workstation-Equipment-QuickStart.md),再查看 [完整架构文档](Workstation-Equipment-Architecture.md)
