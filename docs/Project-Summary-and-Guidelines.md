# Plant01 项目开发指南与架构汇总

## 1. 项目概述
**项目名称**: Plant01.WpfUI / Plant01.Upper
**核心目标**: 开发一个基于 WPF 的工业自动化通用框架，其中 UI 控件库 (`Plant01.WpfUI`) 需严格复刻 **Ant Design 5.x** 的视觉风格和交互体验。
**技术栈**:
- **框架**: .NET 10
- **UI**: WPF (Windows Presentation Foundation)
- **MVVM**: CommunityToolkit.Mvvm
- **依赖注入/宿主**: Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection
- **ORM**: Entity Framework Core (PostgreSQL)
- **日志**: Serilog / NLog
- **映射**: AutoMapper

## 2. 架构设计
本项目采用 **依赖倒置 (Dependency Inversion)** 的 **领域驱动设计 (DDD)** 架构，并结合了 **事件驱动 (Event-Driven)** 模式。

### 核心原则
- **UI 与逻辑分离**: `Presentation.Core` 定义抽象，`Wpf.Core` 或其他 UI 框架实现具体展现。
- **依赖注入**: 通过 `Bootstrapper` 统一管理服务注册，实现模块解耦。
- **独立设备通信**: 采用仿 KEPServer 架构的 **DeviceCommunication** 层。
    - **三层架构**: Channel (驱动/通道) -> Device (设备连接) -> Tag (数据点)。
    - **特性**: 支持多驱动插件、配置热重载、O(1) 高速标签寻址、强类型数据处理。
- **双总线机制**:
    - **应用消息总线**: 使用 `WeakReferenceMessenger` (CommunityToolkit.Mvvm) 处理高频硬件信号（PLC/扫码枪）与业务服务的解耦。核心组件为 `TriggerDispatcherService`。
    - **领域事件总线**: 使用 `IDomainEventBus` 处理业务实体状态变更后的副作用（如：托盘满垛 -> 触发 MES 报工）。
- **工位-设备-标签架构**: 采用 **Workstation -> Equipment -> TagMapping** 的业务层模型，叠加在通讯层 (Channel -> Device -> Tag) 之上。
    - **业务层**: `Workstation` (工位) 包含多个 `Equipment` (设备)，`EquipmentTagMapping` 将业务设备映射到通讯层标签。
    - **自动生成**: 支持从 **DB Schema** (JSON定义) 或 **Rules** (规则批量) 两种模式自动生成标签配置。
    - **依赖倒置**: `ITagGenerationService` 接口位于应用层，实现位于基础设施层，避免循环依赖。
- **Ant Design 风格**: UI 库必须严格遵循 Ant Design 5.x 的设计令牌（Design Tokens）系统。

## 2.5 产线-工段-工位架构 (生产线建模)

本项目采用 **三层产线模型** 进行生产线建模，通过配置文件驱动，内存高效维护：

### 架构层级
```
产线 (ProductionLine)
  └─ 工段 (ProductionSection) [Sequence排序]
      ├─ 分配策略 (StrategyConfigJson)
      └─ 工位 (Workstation) [Sequence排序]
          └─ 设备 (Equipment) [Sequence排序]
              └─ 标签映射 (EquipmentTagMapping)
```

### 核心特点
1. **配置驱动**: 通过 `production_lines.json` 定义产线拓扑，通过 `equipment_templates.json` + `equipment_mappings.json` 定义设备
2. **配置分离**: 设备定义与产线拓扑分离，`production_lines.json` 使用 `EquipmentRefs` 引用设备
3. **内存管理**: 使用 `ProductionConfigManager` 和 `EquipmentConfigService` 在内存中高效维护所有配置，零数据库开销
4. **程序启动时初始化**: `ProductionLineConfigService` 作为 `BackgroundService` 在启动时加载配置
5. **高效查询**: 提供丰富的查询方法（按Code、按关系、按能力等），支持LINQ内存查询

### 关键设计决策
- **为什么不用数据库**: 产线/工段/工位/设备结构是**静态配置**，启动时一次加载，运行时**不需要动态修改**。用内存方案性能更优（无SQL IO）
- **为什么要工段**: 工段是**策略配置层**，可在 `StrategyConfigJson` 中存储"哪些包装机对应哪个码垛机"等分配规则
- **工位状态是运行时的**: `Workstation.Status`、`Workstation.CurrentBatchCode` 等状态字段在内存中更新，用于**流程调度和二维码追踪**
- **⭐ 为什么分离设备配置**: 设备定义可被多个产线/工位复用，独立配置便于维护和版本控制，避免 `production_lines.json` 过于庞大

### 配置文件结构
#### 1. 产线拓扑配置 (`production_lines.json`)
- **位置**: `src/Plant01.Upper.Infrastructure/Configs/production_lines.json`
- **内容**: 定义产线-工段-工位层级，工位使用 `EquipmentRefs` 字符串数组引用设备
- **示例**:
  ```json
  {
    "Code": "Line01",
    "Sections": [{
      "Code": "SEC_PACKAGE",
      "Workstations": [{
        "Code": "L1_WS_PKG01",
        "EquipmentRefs": ["L1_BP01", "L1_PM01"]  // 引用设备模板
      }]
    }]
  }
  ```

#### 2. 设备模板配置 (`equipment_templates.json`)
- **位置**: `src/Plant01.Upper.Infrastructure/Configs/Equipments/equipment_templates.json`
- **内容**: 定义所有设备的基础信息（Code, Name, Type, Capabilities, Sequence）
- **示例**:
  ```json
  [
    {
      "Code": "L1_BP01",
      "Name": "1#线-1号上袋机",
      "Type": "BagPicker",
      "Capabilities": "Heartbeat, AlarmReport",
      "Sequence": 1,
      "Enabled": true
    }
  ]
  ```

#### 3. 设备标签映射配置 (`equipment_mappings.json`)
- **位置**: `src/Plant01.Upper.Infrastructure/Configs/Equipments/equipment_mappings.json`
- **内容**: 定义设备到通信标签的映射关系，包含 `Purpose`（业务用途）和 `IsCritical`（关键标签标识）
- **示例**:
  ```json
  [
    {
      "EquipmentCode": "L1_BP01",
      "TagMappings": [
        {
          "TagName": "SDJ01.HeartBreak",
          "Purpose": "Heartbeat",
          "IsCritical": true,
          "ChannelName": "PLC01",
          "Remarks": "设备心跳信号"
        }
      ]
    }
  ]
  ```

### 设备-标签映射的业务语义

#### TagPurpose（标签用途）
**作用**: 标识标签在业务逻辑中的语义角色，使业务代码可以按用途筛选和处理标签

**预定义用途** (位于 `EquipmentTagMapping.cs`):
- `Heartbeat` - 心跳信号
- `Alarm` / `AlarmCode` - 报警状态/代码
- `OutputCount` - 产量统计
- `Status` / `Mode` - 设备状态/模式
- `Quality` / `Recipe` - 质量数据/配方
- `Speed` / `Temperature` / `Pressure` - 工艺参数

**业务层应用示例**:
```csharp
// 场景1: 心跳监控 - 自动筛选所有心跳标签
var heartbeatMappings = equipment.TagMappings
    .Where(m => m.Purpose == TagPurpose.Heartbeat);
foreach (var mapping in heartbeatMappings)
{
    var isAlive = tagEngine.GetTagValue<bool>(mapping.TagName);
    if (!isAlive) 
        _logger.LogWarning($"设备 {equipment.Code} 心跳丢失");
}

// 场景2: 产量统计 - 按用途聚合
var totalOutput = equipment.TagMappings
    .Where(m => m.Purpose == TagPurpose.OutputCount)
    .Sum(m => tagEngine.GetTagValue<int>(m.TagName));

// 场景3: 报警路由 - 根据用途分发到不同处理器
if (mapping.Purpose == TagPurpose.Alarm)
    await _alarmService.HandleAlarmAsync(equipment, tagValue);
```

#### IsCritical（关键标签标识）
**作用**: 标记关键业务标签，用于差异化监控策略和告警升级

**业务层应用示例**:
```csharp
// 场景1: 差异化扫描频率
var criticalTags = equipment.TagMappings.Where(m => m.IsCritical);
var normalTags = equipment.TagMappings.Where(m => !m.IsCritical);
await ScanTagsAsync(criticalTags, scanRate: 100);   // 关键标签100ms
await ScanTagsAsync(normalTags, scanRate: 500);     // 普通标签500ms

// 场景2: 故障告警升级
if (mapping.IsCritical && hasError)
    await _notificationService.SendUrgentAlert(equipment, error);

// 场景3: 历史数据优先级
if (mapping.IsCritical)
    await _historianService.StoreWithHighPriority(tag, value);
```

#### 为什么不直接写入 tags.csv？
**推荐**: 将 `Purpose` 和 `IsCritical` 保留在 `equipment_mappings.json` 中

**原因**:
- ✅ **分层清晰**: `tags.csv` 属于通信层配置（地址、数据类型），`equipment_mappings.json` 属于业务层配置（用途、优先级）
- ✅ **复用灵活**: 同一个 PLC 标签可能被多个设备引用，业务用途可能不同（如 DB200.0 既是上袋机心跳，也是包装机心跳）
- ✅ **维护解耦**: 修改业务逻辑（调整用途/优先级）不需要重新生成通信配置

**如果确实要合并到 tags.csv** (不推荐):
可以在 CSV 中添加 `Purpose` 和 `IsCritical` 列，但会失去上述灵活性。

### 查询接口
```csharp
ProductionConfigManager configManager = ...; // DI注入

// 按Code查询
var equipment = configManager.GetEquipmentByCode("L1_BP01");
var workstation = configManager.GetWorkstationByCode("L1_WS_PKG01");
var section = configManager.GetSectionByCode("SEC_PACKING");

// 按关系查询
var workstations = configManager.GetWorkstationsBySection("SEC_PACKING");
var equipments = configManager.GetEquipmentsByWorkstation("L1_WS_PKG01");
var section = configManager.GetSectionByEquipment("L1_BP01");

// 查询分配策略
var strategyJson = configManager.GetSectionStrategyJson("SEC_PALLETIZING");

// 配置统计
var summary = configManager.GetConfigSummary(); // "产线数: 2, 工段数: 10, 工位数: 18, 设备数: 30"
```

## 3. 项目结构说明

### 表现层 (Presentation Layer)
| 项目名称 | 职责说明 |
| :--- | :--- |
| **Plant01.Upper.Wpf** | **应用程序入口**。包含 `App.xaml`，主要负责启动宿主环境，几乎不包含业务逻辑。 |
| **Plant01.Upper.Wpf.Core** | **WPF 具体实现层**。实现了 `Presentation.Core` 中定义的抽象接口，负责 WPF 特有的视图逻辑。 |
| **Plant01.WpfUI** | **核心 UI 控件库**。基于 Ant Design 5.x 风格开发。包含自定义控件、样式、模板。**核心要求：使用 `ThemeManager` 和 `DesignTokenKeys` 管理样式。** |
| **Plant01.WpfUI.ViewModel** | **业务 UI 组件库**。包含依赖于 `Plant01.Core` 或特定 ViewModel 逻辑的复杂 UI 控件（如 `DynamicEntityList`）。此项目将纯粹的 UI 样式库 (`Plant01.WpfUI`) 与业务逻辑解耦。 |
| **Plant01.Upper.Presentation.Core** | **UI 抽象层**。包含 ViewModels、UI 接口定义。不依赖具体 UI 框架（如 WPF），以便未来适配 Avalonia, MAUI, Blazor 等。 |
| **Plant01.Upper.Presentation.Bootstrapper** | **聚合根 / 组合根**。负责应用程序启动时的服务注册（DI 容器配置）。独立存在是为了让不同的 UI 实现层（如 WPF, MAUI）复用相同的业务服务注册逻辑。 |

### 领域与应用层 (Domain & Application Layer)
| 项目名称 | 职责说明 |
| :--- | :--- |
| **Plant01.Upper.Application** | **应用服务层**。包含业务用例实现 (`Services`)、DTOs、接口定义 (`Interfaces`)、验证逻辑 (`Validators`)。**核心服务**: `ITagGenerationService` (标签生成服务接口)。 |
| **Plant01.Upper.Domain** | **领域层**。包含实体 (`Entities`)、值对象 (`ValueObjects`)、领域事件 (`Events`)、仓储接口 (`Repository`)。**关键实体**: `Workstation` (工位), `Equipment` (设备), `EquipmentTagMapping` (标签映射)。 |
| **Plant01.Domain.Shared** | **共享领域层**。包含跨多个限界上下文共享的枚举、常量、基础接口。**关键枚举**: `EquipmentType` (10种设备类型), `Capabilities` (8种设备能力 Flags)。 |

### 基础设施层 (Infrastructure Layer)
| 项目名称 | 职责说明 |
| :--- | :--- |
| **Plant01.Upper.Infrastructure** | **基础设施实现**。包含数据仓储实现 (`Repository`)、数据库上下文、**消息调度 (`TriggerDispatcher`)**、**领域事件总线 (`DomainEventBus`)**、**设备通信核心 (DeviceCommunication)**。**⭐ 架构重构** (2025-12-19): ① **分层解耦** - 将通信层 `Tag` 重构为 `CommunicationTag`（包含 ChannelName/DeviceName/Address），Domain 层使用轻量级 `TagValue`（纯领域模型）。② **强类型配置** - `ConfigurationLoader`（JsonDocument解析）、`SiemensS7Config/ModbusTcpConfig/SimulationConfig`（强类型配置类）、`DeviceConfigExtensions`（验证扩展）、`channel-config.schema.json`（JSON Schema）。③ **通道-设备正交** - 修复架构错误，1个 Channel 管理多个 Device（而非每个 Device 一个 Channel）。**其他**: `TagGenerationServiceImpl`（标签生成）、`S7AddressScanner`（地址扫描）、`S7AddressParser`（地址解析）、`SiemensS7Driver`（西门子S7驱动，支持**智能批量读取**与**自动降级**）。 |
| **Plant01.Infrastructure.Shared** | **共享基础设施**。包含通用服务实现（如 `HttpService`）、扩展方法。 |
| **Plant01.Core** | **核心工具库**。包含基础框架代码、通用工具类 (`Utilities`)、扩展方法 (`Extensions`)。 |

### 演示与测试
| 项目名称 | 职责说明 |
| :--- | :--- |
| **wpfuidemo** | **UI 控件库演示程序**。所有 `Plant01.WpfUI` 开发的控件必须在此项目中进行演示和测试，确保功能和样式符合 Ant Design 5.x 标准。 |

## 4. UI 开发规范 (Plant01.WpfUI)

### 4.1 设计系统 (Theming System)
本项目使用一套自定义的主题管理系统来模拟 Ant Design 的 Token 系统。

- **ThemeManager**: 位于 `Plant01.WpfUI.Helpers`。负责在运行时应用主题（Light/Dark），生成调色板，并将颜色映射到资源字典中。
- **DesignTokenKeys**: 位于 `Plant01.WpfUI.Themes`。定义了所有可用的资源键（ComponentResourceKey）。
    - 例如：`DesignTokenKeys.PrimaryColor`, `DesignTokenKeys.ColorPrimaryBg`, `DesignTokenKeys.BorderColor`。

### 4.2 样式开发要求
1.  **禁止硬编码颜色**: 严禁在 XAML 或代码中直接使用 `#FFFFFF` 或 `Colors.Red` 等固定颜色。
2.  **使用 DynamicResource**: 所有颜色、画笔必须通过 `DynamicResource` 引用 `DesignTokenKeys` 定义的键。
    ```xml
    <!-- 正确示例 -->
    <Border Background="{DynamicResource {x:Static themes:DesignTokenKeys.ComponentBackground}}"
            BorderBrush="{DynamicResource {x:Static themes:DesignTokenKeys.BorderColor}}" />
    ```
3.  **复刻 Ant Design**: 在开发新控件时，请对照 [Ant Design 官方文档](https://ant.design/components/overview-cn/)，确保：
    - 颜色使用正确的 Token（如 Hover, Active 状态）。
    - 圆角、阴影、间距符合规范。
    - 交互动画（如 Ripple 效果、过渡动画）尽可能还原。

## 5. 关键文件索引

### 产线-工段-工位架构（内存式配置管理 + 配置分离）
- **产线实体**: `src/Plant01.Upper.Domain/Entities/ProductionLine.cs`
- **工段实体**: `src/Plant01.Upper.Domain/Entities/ProductionSection.cs`
- **工位实体**: `src/Plant01.Upper.Domain/Entities/Workstation.cs`
- **设备实体**: `src/Plant01.Upper.Domain/Entities/Equipment.cs` (TagMappings标记为[NotMapped]，运行时加载)
- **标签映射**: `src/Plant01.Upper.Domain/Entities/EquipmentTagMapping.cs` (纯内存对象，包含Purpose和IsCritical)
- **⭐ 配置管理器**: `src/Plant01.Upper.Application/Services/ProductionConfigManager.cs` (产线拓扑查询接口)
- **⭐ 设备配置服务**: `src/Plant01.Upper.Infrastructure/Services/EquipmentConfigService.cs` (设备模板和映射加载，支持热重载)
- **⭐ 配置DTO**: `src/Plant01.Upper.Infrastructure/Configs/Models/EquipmentConfigDto.cs` (EquipmentTemplateDto, EquipmentMappingDto)
- **配置加载服务**: `src/Plant01.Upper.Infrastructure/Services/ProductionLineConfigService.cs` (BackgroundService，启动时加载并集成设备配置)

### 核心服务与基础设施
- **服务注册**: `src/Plant01.Upper.Presentation.Bootstrapper/Bootstrapper.cs`
- **事件注册**: `src/Plant01.Upper.Presentation.Bootstrapper/EventRegistrationService.cs`
- **触发器调度**: `src/Plant01.Upper.Infrastructure/Services/TriggerDispatcherService.cs`
- **设备通信服务**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceCommunicationService.cs` (⭐ 已重构为 1通道→N设备)
- **通道管理**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Engine/Channel.cs` (包含内部 DeviceConnection 类)
- **标签引擎**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Engine/TagEngine.cs` (使用 CommunicationTag)
### 设备通信架构 (⭐ 重构 - 2025-12-19)
#### 分层模型
- **⭐ 通信标签**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Models/CommunicationTag.cs` (Infrastructure 层，包含通信属性)
- **⭐ 领域标签值**: `src/Plant01.Upper.Domain/Models/TagValue.cs` (Domain 层，纯领域模型)
- **⭐ 配置加载器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/ConfigurationLoader.cs` (使用 CommunicationTag)
- **⭐ CSV 映射**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/TagMap.cs` (映射为 CommunicationTag)

#### 强类型配置系统
- **强类型配置类**:
  - `src/Plant01.Upper.Infrastructure/DeviceCommunication/DriverConfigs/SiemensS7Config.cs`
  - `src/Plant01.Upper.Infrastructure/DeviceCommunication/DriverConfigs/ModbusTcpConfig.cs`
  - `src/Plant01.Upper.Infrastructure/DeviceCommunication/DriverConfigs/SimulationConfig.cs`
- **验证扩展**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Extensions/DeviceConfigExtensions.cs`
- **JSON Schema**: `src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/Schemas/channel-config.schema.json`

#### 驱动实现
- **西门子 S7**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs` (使用 CommunicationTag)
- **仿真驱动**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SimulationDriver.cs` (使用 CommunicationTag)
- **配置文件目录**: `src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/`
  - `Channels/` - 通道配置JSON
  - `Schemas/` - JSON Schema定义
  - `Tags/` - 标签CSV文件
  - `README.md` - 配置结构说明hema定义
  - `Tags/` - 标签CSV文件
  - `README.md` - 配置结构说明

### 工位-设备-标签架构
- **标签映射**: `src/Plant01.Upper.Domain/Entities/EquipmentTagMapping.cs`
- **设备类型枚举**: `src/Plant01.Domain.Shared/Models/Equipment/EquipmentType.cs` (已新增 `StretchWrapper = 11` 缠绕机)
- **设备能力枚举**: `src/Plant01.Domain.Shared/Models/Equipment/Capabilities.cs` (8种能力: Heartbeat, AlarmReport, OutputCount等)
- **标签生成接口**: `src/Plant01.Upper.Application/Services/TagGenerationService.cs`
- **标签生成实现**: `src/Plant01.Upper.Infrastructure/Services/TagGenerationServiceImpl.cs`
- **配置加载器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/ConfigurationLoader.cs`
- **S7地址扫描器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressScanner.cs`
- **S7地址解析器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressParser.cs`
- **S7驱动实现**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs`

### UI 主题系统
- **主题管理**: `src/Plant01.WpfUI/Helpers/ThemeManager.cs`
- **设计令牌**: `src/Plant01.WpfUI/Themes/DesignTokenKeys.cs`
### 文档
- **⭐ 项目开发指南**: `docs/Project-Summary-and-Guidelines.md` (本文档，包含架构汇总和开发规范)
- **设备通信架构** (2025-12-19重构):
  - 强类型配置实现: `docs/DeviceCommunication-StrongTypedConfig-Implementation.md`
  - 快速指南: `docs/DeviceCommunication-AddNewDriver-QuickGuide.md`
  - 测试计划: `docs/DeviceCommunication-Testing-Plan.md`
  - **架构分层说明**: 见本文档第 7.1 节（CommunicationTag vs TagValue）
- **工位设备架构**:
  - 快速入门: `docs/Workstation-Equipment-QuickStart.md`
  - 完整说明: `docs/Workstation-Equipment-Architecture.md`
- **架构索引**: `docs/README-Architecture-Index.md`quipments/equipment_templates.json` - 设备模板定义
  - `src/Plant01.Upper.Infrastructure/Configs/Equipments/equipment_mappings.json` - 设备标签映射(含Purpose和IsCritical)
- **Schema示例**: `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB1.schema.json`

### 文档
- **⭐ 设备通信强类型配置** (新增):
  - 完整实现: `docs/DeviceCommunication-StrongTypedConfig-Implementation.md`
  - 快速指南: `docs/DeviceCommunication-AddNewDriver-QuickGuide.md`
  - 测试计划: `docs/DeviceCommunication-Testing-Plan.md`
- **工位设备架构**:
  - 快速入门: `docs/Workstation-Equipment-QuickStart.md`
  - 完整说明: `docs/Workstation-Equipment-Architecture.md`
- **架构索引**: `docs/README-Architecture-Index.md`

## 6. 开发流程建议
1.  **新增控件**: 在 `Plant01.WpfUI` 中创建控件类和 Generic.xaml 样式。
2.  **定义样式**: 使用 `DesignTokenKeys` 定义视觉外观。
3.  **演示验证**: 在 `wpfuidemo` 项目中添加该控件的演示页面，验证 Light/Dark 模式切换效果。
4.  **业务集成**: 在 `Plant01.Upper.Wpf` 或其他具体应用中使用该控件。

## 7. 设备通信开发规范 (Device Communication Standards)

### 7.1 架构模型
系统采用 **Channel -> Device -> Tag** 的三层层级结构，严格区分驱动逻辑与设备连接参数。

- **Channel (通道)**: 定义驱动类型（如 SiemensS7, ModbusTCP）。**⭐ 1个通道管理多个设备**（共享驱动逻辑）。
- **Device (设备)**: 定义具体的连接参数（如 IP, Port, Slot, Rack）。**每个设备独立连接和轮询**。
- **Tag (标签)**: 定义数据点属性（地址, 数据类型, 读写权限）。

**⭐ 架构分层** (2025-12-19重构):
- **Infrastructure 层**: `CommunicationTag` 类（包含 ChannelName、DeviceName、Address 等通信层概念）
- **Domain 层**: `TagValue` 结构体（纯领域模型，仅包含 Name、Value、Quality、Timestamp）
- **Application 层**: `IDeviceCommunicationService` 接口返回 `TagValue`（而非通信层的 `TagData`）

**依赖方向**: Domain ← Application ← Infrastructure（符合 DDD 依赖倒置原则）

### 7.2 配置规范

#### 7.2.1 通道与设备配置 (JSON)
**⭐ 已实现强类型配置系统** (2025-12-19更新)

系统现已支持驱动特定的强类型配置类 + JSON Schema验证：

**配置文件结构**:
```
Configs/DeviceCommunications/
  ├─ Channels/          # 通道配置JSON文件
  │   ├─ SiemensS7Tcp.json
  │   ├─ ModbusTcp.json
  │   └─ Simulation.json
  ├─ Schemas/           # JSON Schema文件
  │   └─ channel-config.schema.json
  ├─ Tags/              # 标签CSV文件
  │   └─ tags.csv
  └─ README.md          # 配置文档
```

**强类型配置类** (位于 `Infrastructure/DeviceCommunication/DriverConfigs/`):
- `SiemensS7Config.cs` - 西门子S7 PLC配置
- `ModbusTcpConfig.cs` - Modbus TCP配置
- `SimulationConfig.cs` - 仿真驱动配置

**配置示例** (`SiemensS7Tcp.json`):
```json
{
  "$schema": "../Schemas/channel-config.schema.json",
  "Name": "SiemensS7Tcp",
  "Description": "西门子S7 TCP通道",
  "Drive": "SiemensS7",
  "Enable": true,
  "Devices": [
    {
      "Name": "PLC01",
      "Description": "1#包装机PLC",
      "Enable": true,
      "Code": "L1_PLC01",
      "IpAddress": "192.168.1.100",
      "Port": 102,
      "Rack": 0,
      "Slot": 1,
      "ScanRate": 100,
      "ConnectTimeout": 5000,
      "PlcModel": "S7_1200"
    }
  ]
}
```

**验证机制**:
1. **JSON Schema** - IDE实时验证(VS Code IntelliSense支持)
2. **DataAnnotations** - 运行时强类型验证

**驱动使用示例**:
```csharp
// 在驱动的ValidateConfig中
public override Task ValidateConfig(DeviceConfig config)
{
    var driverConfig = config.GetAndValidateDriverConfig<SiemensS7Config>();
    // 自动验证IP格式、端口范围、必填字段
    return Task.CompletedTask;
}

// 在ConnectAsync中使用强类型配置
public override async Task<bool> ConnectAsync()
{
    var driverConfig = _config.GetDriverConfig<SiemensS7Config>();
    var ip = driverConfig.IpAddress;  // 直接属性访问,类型安全
    var port = driverConfig.Port;
    var timeout = driverConfig.ConnectTimeout;  // 不再硬编码
    // ...
}
```

**关键类**:
- `ConfigurationLoader.cs` - 重构为使用`JsonDocument`解析,修复Devices数组读取问题
- `DeviceConfigExtensions.cs` - 提供`GetAndValidateDriverConfig<T>()`扩展方法

### 7.3 开发注意事项
1.  **类型安全**: 应用层获取数据时，**必须**使用泛型方法 `GetTagValue<T>("TagName")`，避免手动拆箱。
2.  **⭐ 架构分层** (2025-12-19新增):
    - **Infrastructure 层**: 使用 `CommunicationTag` 类进行底层通信（包含 ChannelName、DeviceName、Address）
    - **Application 层**: `IDeviceCommunicationService` 接口返回 `TagValue` 结构体（纯领域模型）
    - **驱动接口**: `IDriver` 使用 `object` 参数，实现层转换为 `CommunicationTag`
    - **命名空间分离**: 避免 `TagDataType`/`AccessRights`/`TagQuality` 冲突，使用 `Models.TagDataType` 限定
3.  **驱动开发**:
    - 必须实现 `Initialize(DeviceConfig)` 和 `ValidateConfig(DeviceConfig)`。
    - ⭐ `ValidateConfig` 中使用 `config.GetAndValidateDriverConfig<TConfig>()` 自动验证驱动配置。
    - ⭐ `ConnectAsync` 等方法中使用 `config.GetDriverConfig<TConfig>()` 获取强类型配置。
    - ⭐ `ReadTagsAsync` 中将 `IEnumerable<object>` 转换为 `CommunicationTag`：`var tag = tagObj as CommunicationTag;`
    - 驱动内部负责将标准 `TagDataType` 映射为协议特定的读取指令。
4.  **通道-设备正交** (⭐ 关键架构):
    - **错误**: 每个 Device 创建一个 Channel 实例
    - **正确**: 每个 ChannelConfig 创建**1个** Channel 实例，内部管理多个 DeviceConnection
    - **示例**: 3个 PLC（同驱动）= 1个 SiemensS7 Channel + 3个 DeviceConnection
5.  **数组处理**: 当 CSV 中 `Length > 1` (且非 String) 时，驱动返回的是数组对象。应用层应使用 `GetTagValue<int[]>` 等方式接收。
6.  **⭐ 配置热重载**: 系统支持运行时监测配置文件变更,自动重新加载并重新连接设备。Boolean`, `String` 等)，严禁使用驱动特定名称（如 `Word`, `Real`）。
- **RW (权限)**: 使用 `R` (只读), `W` (只写), `RW` (读写)。
- **Length**: 对于数组，指定数组长度；对于字符串，指定字节长度。

### 7.3 开发注意事项
1.  **类型安全**: 应用层获取数据时，**必须**使用泛型方法 `GetTagValue<T>("TagName")`，避免手动拆箱。
2.  **驱动开发**:
    - 必须实现 `Initialize(DeviceConfig)` 和 `ValidateConfig(DeviceConfig)`。
    - ⭐ **新规范**: `ValidateConfig` 中使用 `config.GetAndValidateDriverConfig<TConfig>()` 自动验证驱动配置。
    - ⭐ **新规范**: `ConnectAsync` 等方法中使用 `config.GetDriverConfig<TConfig>()` 获取强类型配置。
    - 驱动内部负责将标准 `TagDataType` 映射为协议特定的读取指令。
3.  **数组处理**: 当 CSV 中 `Length > 1` (且非 String) 时，驱动返回的是数组对象。应用层应使用 `GetTagValue<int[]>` 等方式接收。
4.  **⭐ 配置热重载**: 系统支持运行时监测配置文件变更,自动重新加载并重新连接设备。

## 8. 工位-设备管理开发规范 (Workstation & Equipment Management)

### 8.1 架构概述 - 三层产线模型 + 内存管理

系统采用 **三层产线模型** + **内存配置管理** 的设计，不使用数据库持久化：

#### 产线-工段-工位-设备层级
```
ProductionLine (产线) - 顶级容器
  ├─ Code: "Line01", Name: "1# 生产线"
  └─ Sections: ProductionSection[]
      ├─ Code: "SEC_PACKAGE", Sequence: 1
      ├─ StrategyConfigJson: 策略配置（JSON）
      └─ Workstations: Workstation[]
          ├─ Code: "L1_WS_PKG01", Sequence: 1
          └─ Equipments: Equipment[]
              ├─ Code: "L1_BP01" (上袋机)
              └─ Code: "L1_PM01" (包装机)
```

#### 为什么不用数据库
- ✅ 产线/工段/工位结构是**完全静态**的，不需要运行时动态修改
- ✅ 配置文件 (`production_lines.json`) 一次性加载到内存
- ✅ 查询完全在内存中进行，性能最优（无SQL IO）
- ✅ 运行时状态（`Status`, `CurrentBatchCode`）存储在内存对象中，用于流程调度

#### ProductionConfigManager - 内存查询引擎
核心服务，提供丰富的查询接口，所有查询都在内存中高效执行：
```csharp
// 按Code查询
GetEquipmentByCode(code)
GetWorkstationByCode(code)
GetSectionByCode(code)
GetProductionLineByCode(code)

// 按关系查询
GetEquipmentsByWorkstation(code)
GetWorkstationsBySection(code)
GetSectionsByProductionLine(code)
GetSectionByEquipment(code)
GetWorkstationByEquipment(code)
GetProductionLineByEquipment(code)

// 查询配置
GetSectionStrategyJson(sectionCode)  // 查询工段分配策略
GetConfigSummary()                   // 统计信息
```

#### 初始化流程
1. `ProductionLineConfigService` 作为 `BackgroundService` 启动时执行
2. 读取 `Configs/production_lines.json` 文件
3. 反序列化为 `List<ProductionLine>`
4. 补全导航属性（ProductionLine → ProductionSection → Workstation → Equipment）
5. 通过 `ProductionConfigManager.LoadFromConfig()` 加载到内存
6. 业务代码通过 DI 注入 `ProductionConfigManager` 进行查询

#### 为什么要工段（ProductionSection）
- **分配策略层**：在 `StrategyConfigJson` 中存储"哪些包装机对应哪个码垛机"等分配规则
  ```json
  {
    "Code": "SEC_PALLETIZING",
    "StrategyConfigJson": "{\"Palletizers\": [{\"Code\":\"L1_PAL01\", \"SourcePackers\":[\"L1_PM01\",\"L1_PM02\",\"L1_PM03\"]}]}"
  }
  ```
- **状态聚合**：可以监控整个工段的状态（所有工位故障 → 工段故障）
- **生产流程**：通过 `Sequence` 字段定义生产流程顺序（1=包装, 2=称重, 3=码垛, 4=缠绕, 5=贴标）

### 8.2 核心实体

#### ProductionLine (产线)
```csharp
public class ProductionLine
{
    public int Id { get; set; }
    public required string Code { get; set; }        // "Line01"
    public required string Name { get; set; }        // "1# 生产线"
    public string? Description { get; set; }
    public bool Enabled { get; set; } = true;
    public List<ProductionSection> Sections { get; set; } = new();  // 包含的工段
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### ProductionSection (工段)
```csharp
public class ProductionSection
{
    public int Id { get; set; }
    public required string Code { get; set; }            // "SEC_PACKING"
    public required string Name { get; set; }            // "包装工段"
    public int Sequence { get; set; }                    // 1,2,3...（流程顺序）
    public string? StrategyConfigJson { get; set; }      // 分配策略（JSON）
    public bool Enabled { get; set; } = true;
    public int ProductionLineId { get; set; }
    public ProductionLine? ProductionLine { get; set; }
    public List<Workstation> Workstations { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### Workstation (工位)

### 8.3 标签自动生成规范

#### 模式1: DB Schema 生成 (推荐用于生产环境)
- **输入**: JSON格式的DB结构定义文件 (`*.schema.json`)
- **内容**:
  ```json
  {
    "DbNumber": 1,
    "Fields": [
      {
        "Name": "WS01_Heartbeat",
        "DataType": "Boolean",
        "Offset": 0,
        "BitIndex": 0,
        "RW": "R",
        "Description": "工位01心跳"
      }
    ]
  }
  ```
- **生成规则**: 
  - Boolean → `DB{N}.DBX{offset}.{bit}`
  - Int16 → `DB{N}.DBW{offset}`
  - Int32/Float → `DB{N}.DBD{offset}`
  - String → `DB{N}.DBS{offset}[length]`
- **位置**: `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/`

#### 模式2: Rules 批量生成 (用于测试/模拟)
- **输入**: `AddressRules` 对象
- **参数**:
  - `DbNumber`: DB块号
  - `NameTemplate`: 命名模板 (如 "Test_Tag_{Index}")
  - `StartOffset`: 起始偏移量
  - `Stride`: 地址步进 (字节)
  - `Count`: 生成数量
  - `DataType`: 数据类型
- **示例**: 生成 50 个测试标签 `Test_Tag_0` ~ `Test_Tag_49`

#### 生成流程
1. **预览**: 调用 `ITagGenerationService.PreviewTagsFromDbSchemaAsync()` 或 `PreviewTagsByRulesAsync()`
2. **验证**: 检查生成的标签列表
3. **合并**: 调用 `MergeGeneratedTagsAsync()` 写入 `tags.csv`
4. **备份**: 系统自动备份为 `.bak` 文件

#### 服务注入
```csharp
// 应用层注入接口
public MyService(ITagGenerationService tagService) { }

// 基础设施层实现在 Bootstrapper 中注册
services.AddSingleton<ITagGenerationService, TagGenerationServiceImpl>();
```

### 8.4 S7 通讯集成

#### HslCommunication 依赖
- **版本**: 12.5.3
- **协议支持**: Siemens S7-200/300/400/1200/1500
- **核心类**: `HslCommunication.Profinet.Siemens.SiemensS7Net`

#### 地址格式规范
| 类型 | 格式 | 示例 | 说明 |
|------|------|------|------|
| Boolean | `DB{N}.DBX{byte}.{bit}` | `DB1.DBX0.0` | 字节0的第0位 |
| Int16 | `DB{N}.DBW{offset}` | `DB1.DBW2` | Word偏移2 |
| Int32/Float | `DB{N}.DBD{offset}` | `DB1.DBD4` | DWord偏移4 |
| String | `DB{N}.DBS{offset}[len]` | `DB1.DBS12[20]` | 从偏移12读取20字节 |

#### 驱动实现要点
- **连接管理**: `ConnectAsync()` 创建 `SiemensS7Net` 实例，配置 IP/Port/Rack/Slot
- **地址解析**: 使用 `S7AddressParser` 将标准地址解析为 `(DbNumber, Offset, BitIndex, DataType, Length)`
- **类型处理**:
  - 标量: 使用 `ReadAsync(address)` 直接读取
  - 数组: 使用 `ReadAsync(address, length)` 批量读取
  - 字符串: 从 DBS 区域读取指定长度字节
- **错误处理**: 所有操作使用 `OperateResult<T>` 包装，检查 `IsSuccess` 后返回 `Content`

### 8.5 开发注意事项

#### 依赖倒置原则
- **应用层**: 仅定义 `ITagGenerationService` 接口，参数类型使用 `object` 避免引用基础设施层
- **基础设施层**: 实现 `TagGenerationServiceImpl`，依赖 `ConfigurationLoader`, `S7AddressScanner`
- **避免循环**: Application 层 **不可** 引用 Infrastructure 层

#### 数据一致性
- **备份机制**: 每次合并前自动备份 `.bak`，保证可回滚
- **原子操作**: 使用文件锁确保并发安全
- **验证检查**: 生成前验证 IP、端口、DB号有效性

#### 性能优化
- **批量生成**: 规则模式可一次生成数千个标签
- **增量合并**: 仅更新变化的标签，保留原有配置
- **异步操作**: 所有生成和扫描操作使用 `async/await`

### 8.6 扩展路线图
- **多协议支持**: Modbus TCP, OPC UA 驱动的标签生成
- **历史数据**: EquipmentHistory 实体记录状态变更
- **报警管理**: AlarmRule 实体定义报警规则
- **工艺配方**: Recipe 实体关联 Equipment，支持远程下发
- **设备分组**: EquipmentGroup 实现逻辑分组 (按区域/产线/功能)
- **UI集成**: WorkstationManagementViewModel, EquipmentMonitoringView

### 8.7 参考文档
- **快速入门**: `docs/Workstation-Equipment-QuickStart.md` (5分钟上手)
- **完整架构**: `docs/Workstation-Equipment-Architecture.md` (8章节详解)
- **文档索引**: `docs/README-Architecture-Index.md` (导航目录)

## 9. 更新日志 (Changelog)

### 2025-12-20 通信层优化与修复
1.  **S7 智能批量读取 (Smart Batch Reading)**:
    -   在 `SiemensS7Driver` 中实现了基于地址连续性的批量读取。
    -   引入 `MaxByteGap` (默认 200 字节) 参数，自动将地址间隙过大的标签拆分为多个请求，避免无效数据读取。
    -   优化了 TCP 包大小，显著提升了大量标签读取时的吞吐量。
2.  **回调机制修复**:
    -   修复了 `Channel` 与 `Driver` 职责不清导致的数据变更回调丢失问题。
    -   **变更**: 移除了 Driver (S7/Modbus) 内部的 `tag.UpdateValue()` 调用。
    -   **规范**: Driver 仅负责返回原始数据，统一由 `Channel` 负责比对旧值、更新 `CommunicationTag` 并触发 `OnTagValueChanged` 事件。
3.  **启动事件控制**:
    -   在 `Channel` 中增加了 `isFirstLoad` 逻辑，支持区分"首次加载"与"运行时变更"（目前默认保留首次触发，以确保 UI 初始化状态正确）。
4.  **乱码修复**:
    -   修复了 `PlcMonitorService.cs` 的文件编码问题，确保中文注释和日志正常显示。
