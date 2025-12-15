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
| **Plant01.Upper.Infrastructure** | **基础设施实现**。包含数据仓储实现 (`Repository`)、数据库上下文、**消息调度 (`TriggerDispatcher`)**、**领域事件总线 (`DomainEventBus`)**、**设备通信核心 (DeviceCommunication)**。**新增**: `TagGenerationServiceImpl` (标签生成实现)、`S7AddressScanner` (S7地址扫描)、`S7AddressParser` (S7地址解析)、`SiemensS7Driver` (西门子S7驱动)、`ConfigurationLoader` (配置加载与标签生成)。 |
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

### 核心服务与基础设施
- **服务注册**: `src/Plant01.Upper.Presentation.Bootstrapper/Bootstrapper.cs`
- **事件注册**: `src/Plant01.Upper.Presentation.Bootstrapper/EventRegistrationService.cs`
- **触发器调度**: `src/Plant01.Upper.Infrastructure/Services/TriggerDispatcherService.cs`
- **设备通信服务**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceCommunicationService.cs`
- **HTTP 服务**: `src/Plant01.Infrastructure.Shared/Services/HttpService.cs`

### 工位-设备-标签架构
- **工位实体**: `src/Plant01.Upper.Domain/Entities/Workstation.cs`
- **设备实体**: `src/Plant01.Upper.Domain/Entities/Equipment.cs`
- **标签映射**: `src/Plant01.Upper.Domain/Entities/EquipmentTagMapping.cs`
- **设备类型枚举**: `src/Plant01.Domain.Shared/Models/Equipment/EquipmentType.cs`
- **设备能力枚举**: `src/Plant01.Domain.Shared/Models/Equipment/Capabilities.cs`
- **标签生成接口**: `src/Plant01.Upper.Application/Services/TagGenerationService.cs`
- **标签生成实现**: `src/Plant01.Upper.Infrastructure/Services/TagGenerationServiceImpl.cs`
- **配置加载器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Configs/ConfigurationLoader.cs`
- **S7地址扫描器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressScanner.cs`
- **S7地址解析器**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressParser.cs`
- **S7驱动实现**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs`

### UI 主题系统
- **主题管理**: `src/Plant01.WpfUI/Helpers/ThemeManager.cs`
- **设计令牌**: `src/Plant01.WpfUI/Themes/DesignTokenKeys.cs`

### 配置文件
- **通道配置**: `src/Plant01.Upper.Infrastructure/Configs/channels.csv`
- **标签配置**: `src/Plant01.Upper.Infrastructure/Configs/tags.csv`
- **Schema示例**: `src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB1.schema.json`

### 文档
- **架构快速入门**: `docs/Workstation-Equipment-QuickStart.md`
- **架构完整说明**: `docs/Workstation-Equipment-Architecture.md`
- **架构索引**: `docs/README-Architecture-Index.md`

## 6. 开发流程建议
1.  **新增控件**: 在 `Plant01.WpfUI` 中创建控件类和 Generic.xaml 样式。
2.  **定义样式**: 使用 `DesignTokenKeys` 定义视觉外观。
3.  **演示验证**: 在 `wpfuidemo` 项目中添加该控件的演示页面，验证 Light/Dark 模式切换效果。
4.  **业务集成**: 在 `Plant01.Upper.Wpf` 或其他具体应用中使用该控件。

## 7. 设备通信开发规范 (Device Communication Standards)

### 7.1 架构模型
系统采用 **Channel -> Device -> Tag** 的三层层级结构，严格区分驱动逻辑与设备连接参数。

- **Channel (通道)**: 定义驱动类型（如 SiemensS7, ModbusTCP）。一个通道可包含多个设备。
- **Device (设备)**: 定义具体的连接参数（如 IP, Port, Slot, Rack）。
- **Tag (标签)**: 定义数据点属性（地址, 数据类型, 读写权限）。

### 7.2 配置规范
- **通道与设备**: 使用 JSON 配置。`Options` 字典存储驱动特定的连接参数。
- **标签定义**: 使用 CSV 统一管理。
    - **DataType**: 必须使用标准类型名称 (`Int16`, `Float`, `Boolean`, `String` 等)，严禁使用驱动特定名称（如 `Word`, `Real`）。
    - **RW (权限)**: 使用 `R` (只读), `W` (只写), `RW` (读写)。
    - **Length**: 对于数组，指定数组长度；对于字符串，指定字节长度。

### 7.3 开发注意事项
1.  **类型安全**: 应用层获取数据时，**必须**使用泛型方法 `GetTagValue<T>("TagName")`，避免手动拆箱。
2.  **驱动开发**:
    - 必须实现 `Initialize(DeviceConfig)` 和 `ValidateConfig(DeviceConfig)`。
    - `ValidateConfig` 中必须校验 IP、端口等关键参数，缺失时抛出异常。
    - 驱动内部负责将标准 `TagDataType` 映射为协议特定的读取指令。
3.  **数组处理**: 当 CSV 中 `Length > 1` (且非 String) 时，驱动返回的是数组对象。应用层应使用 `GetTagValue<int[]>` 等方式接收。

## 8. 工位-设备管理开发规范 (Workstation & Equipment Management)

### 8.1 架构概述
系统采用 **双层模型** 设计，将业务语义与通讯实现分离：

#### 通讯层 (底层 - KEPServer架构)
```
Channel (通道/驱动) -> Device (设备连接) -> Tag (数据点)
```
- **职责**: 处理底层通讯协议 (S7, Modbus, OPC UA 等)
- **特点**: 标签寻址 O(1)、配置热重载、多驱动插件化

#### 业务层 (上层 - 工位设备模型)
```
Workstation (工位) -> Equipment (设备) -> EquipmentTagMapping (标签映射)
```
- **职责**: 业务语义建模、设备状态管理、工位逻辑
- **特点**: 多对多映射、动态绑定、类型化能力

### 8.2 核心实体

#### Workstation (工位)
- **定义**: 生产线上的独立作业单元，可包含多个设备
- **关键属性**:
  - `Code`: 工位编码 (唯一标识)
  - `SectionCode`: 所属工段编码
  - `Status`: 工位状态 (Running/Stopped/Fault/Maintenance)
  - `Equipments`: 包含的设备集合
- **方法**: `AddEquipment()`, `RemoveEquipment()`, `UpdateStatus()`

#### Equipment (设备)
- **定义**: 物理设备实例，具有类型、能力、状态
- **关键属性**:
  - `Code`: 设备编码 (唯一标识)
  - `EquipmentType`: 设备类型枚举 (10种: BagPicker, PackageMachine, Weigher, Robot, AGV 等)
  - `Capabilities`: 设备能力 Flags (8种: Heartbeat, AlarmReport, OutputCount, RemoteControl 等)
  - `Status`: 设备状态 (Running/Stopped/Fault/Maintenance/Offline)
  - `TagMappings`: 标签映射集合
- **方法**: `AddTagMapping()`, `RemoveTagMapping()`, `UpdateCapabilities()`, `UpdateStatus()`

#### EquipmentTagMapping (标签映射)
- **定义**: 业务设备与通讯层标签的关联关系
- **关键属性**:
  - `TagName`: 关联的通讯层标签名称 (如 "WS01_Heartbeat")
  - `Purpose`: 用途分类 (常量: "Heartbeat", "AlarmCode", "OutputCount", "RecipeNo" 等)
  - `Direction`: 数据流向 (R/W/RW)
- **设计原则**: 通过 `TagName` 字符串动态关联，支持热配置

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
