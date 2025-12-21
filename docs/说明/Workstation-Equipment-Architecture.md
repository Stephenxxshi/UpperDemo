# 工位-工站-设备架构设计指南

## 1. 架构概览

本项目采用 **工位/工站 (Workstation) -> 设备 (Equipment) -> 标签 (Tag)** 的三层业务模型，与底层 **Channel -> Device -> Tag** 通信架构解耦。

```
业务层:  Workstation (工位/工站)
           └── Equipment (设备)
                 └── EquipmentTagMapping (标签映射)

通信层:  Channel (通道)
           └── Device (PLC连接)
                 └── Tag (数据点)
```

## 2. 核心实体

### 2.1 Workstation（工位/工站）
**文件**: `src/Plant01.Upper.Domain/Entities/Workstation.cs`

- **职责**: 代表生产线上的一个工作单元（如"1#上袋机工位"）
- **关键字段**:
  - `Code`: 工站编号（唯一标识，如 "WS01"）
  - `Name`: 工站名称
  - `SectionCode`: 所属工段/区域
  - `Status`: 工站状态（Idle/Running/Alarm/Maintenance）
  - `Equipments`: 包含的设备列表

### 2.2 Equipment（设备）
**文件**: `src/Plant01.Upper.Domain/Entities/Equipment.cs`

- **职责**: 代表具体的业务设备（如"SDJ01 上袋机"）
- **关键字段**:
  - `Code`: 设备编号（唯一标识，如 "SDJ01"）
  - `Name`: 设备名称
  - `Type`: 设备类型（`EquipmentType` 枚举）
  - `Capabilities`: 设备能力（`Capabilities` Flags 枚举）
  - `WorkstationId`: 所属工站
  - `Status`: 设备状态（Offline/Online/Running/Alarm/Fault/Maintenance）
  - `TagMappings`: 关联的标签映射列表

### 2.3 EquipmentTagMapping（设备-标签映射）
**文件**: `src/Plant01.Upper.Domain/Entities/EquipmentTagMapping.cs`

- **职责**: 将业务设备与通信层标签关联，明确标签用途
- **关键字段**:
  - `EquipmentId`: 关联设备
  - `TagName`: 通信层标签全名（如 `SDJ01.Heartbeat`）
  - `Purpose`: 标签用途（Heartbeat/Alarm/OutputCount/Mode/Recipe...）
  - `ChannelName`: 通道名称（可选）
  - `IsCritical`: 是否为关键标签

### 2.4 EquipmentType（设备类型枚举）
**文件**: `src/Plant01.Domain.Shared/Models/Equipment/EquipmentType.cs`

```csharp
public enum EquipmentType
{
    BagPicker = 1,      // 上袋机
    PackageMachine = 2, // 包装机
    Weigher = 3,        // 称重机
    InkjetPrinter = 4,  // 喷码机
    LabelPrinter = 5,   // 贴标机
    Palletizer = 6,     // 码垛机
    Conveyor = 7,       // 输送线
    RobotArm = 8,       // 机械臂
    VisionSystem = 9,   // 视觉系统
    MesGateway = 10     // MES网关
}
```

### 2.5 Capabilities（设备能力枚举）
**文件**: `src/Plant01.Domain.Shared/Models/Equipment/Capabilities.cs`

```csharp
[Flags]
public enum Capabilities
{
    None = 0,
    Heartbeat = 1 << 0,         // 心跳监控
    AlarmReport = 1 << 1,       // 报警上报
    OutputCount = 1 << 2,       // 产量统计
    ModeStatus = 1 << 3,        // 模式状态
    RecipeDownload = 1 << 4,    // 配方下发
    ParameterReadWrite = 1 << 5,// 参数读写
    QualityCheck = 1 << 6,      // 质量检测
    PowerStatus = 1 << 7        // 电源状态
}
```

## 3. S7 标签自动生成

### 3.1 核心服务
**文件**: `src/Plant01.Upper.Application/Services/TagGenerationService.cs`

提供以下功能：
- `PreviewFromDbSchema(dbNumber)`: 从 DB Schema 生成标签预览（优先）
- `PreviewFromRules(rules)`: 按规则生成标签预览（回退）
- `MergeToCsv(scannedTags)`: 合并标签到 `tags.csv`（自动备份）
- `TestS7ConnectionAsync(ip, port, rack, slot)`: 测试 S7 连接

### 3.2 两种生成模式

#### 模式1: DB 结构模式（优先）
**配置位置**: `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB{n}.schema.json`

**示例**: `DB1.schema.json`
```json
{
  "DbNumber": 1,
  "Fields": [
    { "Name": "Heartbeat",    "DataType": "Boolean", "Offset": 0,  "BitIndex": 0, "Length": 1, "RW": "R" },
    { "Name": "OutputCount",  "DataType": "Int32",   "Offset": 2,               "Length": 1, "RW": "R" },
    { "Name": "AlarmCode",    "DataType": "Int16",   "Offset": 10,              "Length": 1, "RW": "R" },
    { "Name": "RecipeNo",     "DataType": "Int16",   "Offset": 12,              "Length": 1, "RW": "RW" },
    { "Name": "ProductName",  "DataType": "String",  "Offset": 100,             "Length": 16, "RW": "R" }
  ]
}
```

**使用**:
```csharp
var tagService = serviceProvider.GetRequiredService<TagGenerationService>();
var preview = tagService.PreviewFromDbSchema(1);
tagService.MergeToCsv(preview);
```

#### 模式2: 规则命名模式（回退）
**示例**:
```csharp
var rules = new AddressRules
{
    DbNumber = 10,
    NameTemplate = "Tag_{Index}",
    StartOffset = 0,
    Stride = 4,
    Count = 10,
    DataType = "Int32"
};
var preview = tagService.PreviewFromRules(rules);
tagService.MergeToCsv(preview);
```

### 3.3 地址规范
- **Boolean**: `DB{n}.DBX{byte}.{bit}` (如 `DB1.DBX0.0`)
- **Int16**: `DB{n}.DBW{offset}` (如 `DB1.DBW2`)
- **Int32/Float**: `DB{n}.DBD{offset}` (如 `DB1.DBD4`)
- **String**: `DB{n}.DBS{offset}[len]` (如 `DB1.DBS100[16]`)

### 3.4 S7 驱动实现
**文件**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs`

- 基于 `HslCommunication` 12.5.3
- 支持连接测试、批量读、单点写
- 支持数值型数组（按类型大小顺序偏移读取）
- 字符串使用 `Tag.ArrayLength` 作为长度参数

## 4. 使用流程

### 4.1 初始化工位与设备
```csharp
var workstation = new Workstation
{
    Code = "WS01",
    Name = "1#上袋机工位",
    SectionCode = "SECTION_BAG",
    Status = WorkstationStatus.Idle
};

var equipment = new Equipment
{
    Code = "SDJ01",
    Name = "1#上袋机",
    Type = EquipmentType.BagPicker,
    Capabilities = Capabilities.Heartbeat | Capabilities.AlarmReport | Capabilities.OutputCount,
    WorkstationId = workstation.Id,
    Status = EquipmentStatus.Offline
};

workstation.Equipments.Add(equipment);
```

### 4.2 配置标签映射
```csharp
var mapping = new EquipmentTagMapping
{
    EquipmentId = equipment.Id,
    TagName = "SDJ01.Heartbeat",
    Purpose = TagPurpose.Heartbeat,
    ChannelName = "PLC01",
    IsCritical = true
};

equipment.TagMappings.Add(mapping);
```

### 4.3 生成并合并 S7 标签
```csharp
// 1. 创建 DB Schema 文件（Configs/DbSchemas/DB1.schema.json）
// 2. 调用生成服务
var tagService = new TagGenerationService(configLoader, logger);
var success = tagService.GenerateAndMergeFromDbSchema(1);
// 3. 自动生成 tags.csv 并备份原文件到 .bak
```

### 4.4 从 tags.csv 初始化设备映射
```csharp
var tags = configLoader.LoadTags();
foreach (var tag in tags.Where(t => t.DeviceName == "SDJ01"))
{
    var mapping = new EquipmentTagMapping
    {
        EquipmentId = equipment.Id,
        TagName = tag.Name,
        Purpose = InferPurpose(tag.Name), // 根据命名规则推断用途
        ChannelName = tag.ChannelName,
        IsCritical = tag.Name.Contains("Heartbeat") || tag.Name.Contains("Alarm")
    };
    equipment.TagMappings.Add(mapping);
}
```

## 5. 架构优势

### 5.1 清晰分层
- **业务层**: 工位/设备/映射 → 关注生产逻辑
- **通信层**: Channel/Device/Tag → 关注硬件通讯
- **中间层**: EquipmentTagMapping → 解耦两层

### 5.2 灵活扩展
- 设备类型枚举化，易于新增
- 能力组合式设计，支持多功能设备
- 标签映射支持跨 Channel 绑定

### 5.3 自动生成
- DB Schema 优先，符合现场实际
- 规则命名回退，适配无符号场景
- 自动备份合并，安全可回滚

### 5.4 状态管理
- 工站级状态（Idle/Running/Alarm/Maintenance）
- 设备级状态（Offline/Online/Running/Alarm/Fault/Maintenance）
- 支持领域事件驱动状态变更

## 6. 后续扩展方向

### 6.1 仓储与应用服务
- `IWorkstationRepository`: 工站增删改查
- `IEquipmentRepository`: 设备增删改查
- `IEquipmentTagMappingRepository`: 映射增删改查
- `WorkstationService`: 工站管理应用服务
- `EquipmentService`: 设备管理应用服务

### 6.2 监控与事件
- 扩展 `PlcMonitorService` 使用 `EquipmentTagMapping` 查询设备关联标签
- 发布设备状态变更领域事件（`EquipmentStatusChangedEvent`）
- 工站状态自动计算（所有设备在线 → 工站运行）

### 6.3 UI 集成
- 在 `Plant01.Upper.Presentation.Core` 创建 `WorkstationManagementViewModel`
- 使用 `Plant01.WpfUI` 的 Tree/Table 控件展示工站-设备层次
- 提供设备实时状态监控与标签映射配置界面

### 6.4 配置导入导出
- 将 `Configs/Devices/*.json` 的设备配置导入为 `Equipment` 实体
- 支持工站-设备配置批量导入/导出（Excel/JSON）
- 设备注册发现机制（扫描 PLC 生成设备清单）

## 7. 关键文件索引

### 领域层
- `src/Plant01.Upper.Domain/Entities/Workstation.cs` - 工站实体
- `src/Plant01.Upper.Domain/Entities/Equipment.cs` - 设备实体
- `src/Plant01.Upper.Domain/Entities/EquipmentTagMapping.cs` - 标签映射
- `src/Plant01.Domain.Shared/Models/Equipment/EquipmentType.cs` - 设备类型枚举
- `src/Plant01.Domain.Shared/Models/Equipment/Capabilities.cs` - 能力枚举

### 应用层
- `src/Plant01.Upper.Application/Services/TagGenerationService.cs` - 标签生成服务

### 基础设施层
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs` - S7 驱动
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressScanner.cs` - 地址扫描器
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressParser.cs` - 地址解析器
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7DbSchema.cs` - DB Schema 模型
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/AddressRules.cs` - 规则模型
- `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/ConfigurationLoader.cs` - 配置加载器

### 配置示例
- `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB1.schema.json` - DB Schema 示例
- `src/Plant01.Upper.Infrastructure/Configs/Channels/*.json` - 通道配置
- `src/Plant01.Upper.Infrastructure/Configs/tags.csv` - 标签清单

## 8. 快速开始

### 8.1 构建项目
```powershell
dotnet restore
dotnet build src/Plant01.Upper.Infrastructure/Plant01.Upper.Infrastructure.csproj -c Debug
```

### 8.2 测试 S7 连接
```csharp
var tagService = new TagGenerationService(configLoader, logger);
var connected = await tagService.TestS7ConnectionAsync("10.168.1.21", 102, 0, 1);
```

### 8.3 生成标签
```csharp
// 从 DB1 Schema 生成
var success = tagService.GenerateAndMergeFromDbSchema(1);

// 或按规则生成
var rules = new AddressRules { DbNumber = 10, NameTemplate = "Tag_{Index}", Count = 10 };
success = tagService.GenerateAndMergeFromRules(rules);
```

### 8.4 查看生成结果
- 原文件备份: `Configs/tags.csv.{timestamp}.bak`
- 合并后文件: `Configs/tags.csv`

---

**设计原则**: 依赖倒置 + 领域驱动 + 配置驱动 + 自动生成  
**核心价值**: 清晰分层 + 灵活扩展 + 安全可靠 + 易于维护
