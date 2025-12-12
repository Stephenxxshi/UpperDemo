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
- **双总线机制**:
    - **应用消息总线**: 使用 `WeakReferenceMessenger` (CommunityToolkit.Mvvm) 处理高频硬件信号（PLC/扫码枪）与业务服务的解耦。核心组件为 `TriggerDispatcherService`。
    - **领域事件总线**: 使用 `IDomainEventBus` 处理业务实体状态变更后的副作用（如：托盘满垛 -> 触发 MES 报工）。
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
| **Plant01.Upper.Application** | **应用服务层**。包含业务用例实现 (`Services`)、DTOs、接口定义 (`Interfaces`)、验证逻辑 (`Validators`)。 |
| **Plant01.Upper.Domain** | **领域层**。包含实体 (`Entities`)、值对象 (`ValueObjects`)、领域事件 (`Events`)、仓储接口 (`Repository`)。 |
| **Plant01.Domain.Shared** | **共享领域层**。包含跨多个限界上下文共享的枚举、常量、基础接口。 |

### 基础设施层 (Infrastructure Layer)
| 项目名称 | 职责说明 |
| :--- | :--- |
| **Plant01.Upper.Infrastructure** | **基础设施实现**。包含数据仓储实现 (`Repository`)、数据库上下文、**消息调度 (`TriggerDispatcher`)**、**领域事件总线 (`DomainEventBus`)**、**PLC 监控服务**。 |
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
- **服务注册**: `src/Plant01.Upper.Presentation.Bootstrapper/Bootstrapper.cs`
- **事件注册**: `src/Plant01.Upper.Presentation.Bootstrapper/EventRegistrationService.cs`
- **触发器调度**: `src/Plant01.Upper.Infrastructure/Services/TriggerDispatcherService.cs`
- **主题管理**: `src/Plant01.WpfUI/Helpers/ThemeManager.cs`
- **设计令牌**: `src/Plant01.WpfUI/Themes/DesignTokenKeys.cs`
- **HTTP 服务**: `src/Plant01.Infrastructure.Shared/Services/HttpService.cs`

## 6. 开发流程建议
1.  **新增控件**: 在 `Plant01.WpfUI` 中创建控件类和 Generic.xaml 样式。
2.  **定义样式**: 使用 `DesignTokenKeys` 定义视觉外观。
3.  **演示验证**: 在 `wpfuidemo` 项目中添加该控件的演示页面，验证 Light/Dark 模式切换效果。
4.  **业务集成**: 在 `Plant01.Upper.Wpf` 或其他具体应用中使用该控件。
