# Plant01 项目开发指南（精简版）

## 1. 项目速览
- 名称：`Plant01.WpfUI` / `Plant01.Upper`
- 目标：提供可复用的工业自动化 WPF 框架，并复刻 Ant Design 5.x 的视觉与交互体验。
- 主要技术：.NET 10、WPF、CommunityToolkit.Mvvm、Microsoft.Extensions.Hosting/DependencyInjection、EF Core（PostgreSQL）、Serilog/NLog、AutoMapper。

## 2. 架构概要
- 模式：DDD + 依赖倒置 + 事件驱动。应用层依赖抽象，基础设施提供实现。
- 消息：`WeakReferenceMessenger` 处理硬件高频信号；`IDomainEventBus` 负责领域事件副作用。
- 通信：`DeviceCommunication` 按 `Channel → Device → Tag` 分层，支持多驱动、热重载、强类型配置。
- 业务建模：工位 (`Workstation`) → 设备 (`Equipment`) → 标签映射 (`EquipmentTagMapping`)，叠加在通信层之上。

## 3. 分层职责速查
| 层级 | 项目 | 说明 |
| :--- | :--- | :--- |
| 表现层 | Plant01.Upper.Wpf、Plant01.Upper.Wpf.Core、Plant01.WpfUI、Plant01.WpfUI.ViewModel、Plant01.Upper.Presentation.Core、Plant01.Upper.Presentation.Bootstrapper | 应用入口、WPF 具体实现、Ant Design 风格控件、业务 UI 组件、跨框架抽象、统一 DI 引导。 |
| 应用/领域 | Plant01.Upper.Application、Plant01.Upper.Domain、Plant01.Domain.Shared | 用例服务、DTO、验证；领域实体/值对象/领域事件；共享枚举与接口。 |
| 基础设施 | Plant01.Upper.Infrastructure、Plant01.Infrastructure.Shared、Plant01.Core | 仓储、设备通信、事件/触发器服务、配置加载；共享设施与工具库。 |
| 演示与测试 | wpfuidemo | 控件演示与验证（Light/Dark）。 |

## 4. 三层产线模型 & 配置
- 拓扑：产线 (`ProductionLine`) → 工段 (`ProductionSection`) → 工位 (`Workstation`) → 设备 (`Equipment`) → 标签映射。
- 配置来源：
  - `src/Plant01.Upper.Infrastructure/Configs/production_lines.json` 定义产线拓扑与 `EquipmentRefs`。
  - `src/Plant01.Upper.Infrastructure/Configs/Equipments/equipment_templates.csv` 描述设备属性。
  - `src/Plant01.Upper.Infrastructure/Configs/Equipments/equipment_mappings.csv` 维护业务用途 (`Purpose`) 与关键标记 (`IsCritical`)。
- 运行时：`ProductionLineConfigService` 启动加载 → `ProductionConfigManager` 在内存中提供查询（按代码、关系、策略等）。
- 设计理由：配置静态且复用度高，使用文件 + 内存方案比数据库更高效、易版本控制。

## 5. 设备通信要点
- 数据模型：基础设施层使用 `CommunicationTag`（包含 Channel/Device/Address），领域层通过 `TagValue`（Name/Value/Quality/Timestamp）。
- 配置目录：`src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/` 下的 `Channels/*.json`、`Tags/*.csv`、`Schemas/channel-config.schema.json`。
- 强类型配置：`SiemensS7Config.cs`、`ModbusTcpConfig.cs`、`SimulationConfig.cs` + `DeviceConfigExtensions` 提供验证与访问；驱动通过 `GetAndValidateDriverConfig<T>()` 读取。
- 通道管理：同一驱动一个 Channel，内部托管多个 `DeviceConnection`，负责标签变更回调与热重载。
- 标签生成：`ITagGenerationService`（应用层）+ `TagGenerationServiceImpl`（基础设施）支持 DB Schema 与规则生成，并合并到 `tags.csv`。

## 6. UI 主题规范
- 主题：`Plant01.WpfUI.Helpers.ThemeManager` 负责 Light/Dark 运行时切换。
- 设计令牌：`Plant01.WpfUI.Themes.DesignTokenKeys` 提供 Ant Design Token 键。
- 样式要求：禁止硬编码颜色，全部通过 `DynamicResource` 引用设计令牌；控件在 `wpfuidemo` 中验证视觉与交互。

## 7. 常用文件索引
- 配置与服务：
  - `src/Plant01.Upper.Application/Services/ProductionConfigManager.cs`
  - `src/Plant01.Upper.Infrastructure/Services/EquipmentConfigService.cs`
  - `src/Plant01.Upper.Infrastructure/Services/ProductionLineConfigService.cs`
  - `src/Plant01.Upper.Infrastructure/DeviceCommunication/DeviceCommunicationService.cs`
  - `src/Plant01.Upper.Infrastructure/DeviceCommunication/Engine/Channel.cs`
- 文档：
  - `docs/DeviceCommunication-StrongTypedConfig-Implementation.md`
  - `docs/DeviceCommunication-AddNewDriver-QuickGuide.md`
  - `docs/Workstation-Equipment-QuickStart.md`
  - `docs/Workstation-Equipment-Architecture.md`
  - `docs/README-Architecture-Index.md`

## 8. 建议的开发步骤
1. 在 `Plant01.WpfUI` 增加控件及样式（遵循 Token）。
2. 在 `wpfuidemo` 创建示例页，验证 Light/Dark 效果。
3. 如需新设备/标签，更新配置文件并通过 `ITagGenerationService` 生成或合并。
4. 在 `Bootstrapper` 注册必要服务与事件。

## 9. 近期更新摘录
- **2025-12-20**：S7 智能批量读取、Channel 负责标签回调、初始化事件控制、中文编码修复。
- **2025-12-24**：混合配置加载（JSON + CSV）、`MultiFormatConfigLoader`/`ConfigParserFactory`、孤立节点告警、配置热重载加强。
