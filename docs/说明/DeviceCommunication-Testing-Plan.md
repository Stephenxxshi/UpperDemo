# DeviceCommunication强类型配置 - 测试计划

## 实现状态

### ✅ 已完成

1. **强类型配置类** (`Plant01.Upper.Infrastructure/DeviceCommunication/DriverConfigs/`)
   - `SiemensS7Config.cs` - 西门子S7 PLC配置类
   - `ModbusTcpConfig.cs` - Modbus TCP配置类
   - `SimulationConfig.cs` - 仿真驱动配置类
   - 所有类包含DataAnnotations验证特性

2. **验证扩展方法** (`Plant01.Upper.Infrastructure/DeviceCommunication/Extensions/`)
   - `DeviceConfigExtensions.cs`
   - `GetAndValidateDriverConfig<T>()` - 自动类型转换和验证
   - 完整的错误信息收集

3. **JSON Schema** (`Configs/DeviceCommunications/Schemas/`)
   - `channel-config.schema.json`
   - Draft 2020-12标准
   - 条件验证(基于Drive字段)
   - IDE IntelliSense支持

4. **配置加载器重构** (`Plant01.Upper.Infrastructure/DeviceCommunication/Configs/`)
   - `ConfigurationLoader.cs`
   - 修复Devices数组解析问题(使用JsonDocument)
   - 添加`ParseDevice()`和`JsonElementToObject()`辅助方法

5. **驱动更新**
   - `SiemensS7Driver.cs` - 完全使用强类型配置
   - `ValidateConfig()` - 使用`GetAndValidateDriverConfig<T>()`
   - `ConnectAsync()` - 直接访问强类型属性

6. **配置文件**
   - `SiemensS7Tcp.json` - 更新为新结构(IpAddress, Rack, Slot等)
   - `Simulation.json` - 仿真驱动示例配置
   - 所有JSON文件包含`$schema`引用

7. **文档**
   - `README.md` - 配置结构详细说明
   - `DeviceCommunication-StrongTypedConfig-Implementation.md` - 完整实现总结
   - `DeviceCommunication-AddNewDriver-QuickGuide.md` - 开发者快速指南
   - `README-Architecture-Index.md` - 架构索引更新

8. **项目构建**
   - ✅ `Plant01.Upper.Infrastructure.csproj` 已更新
   - ✅ 移除所有旧配置文件引用
   - ✅ 使用通配符模式 `Configs\DeviceCommunications\**\*.json`
   - ✅ 编译成功(4个nullable警告,不影响功能)

---

## 测试计划

### 1. 配置加载测试

#### 1.1 验证Devices数组解析
**目标**: 确认ConfigurationLoader能正确解析Devices数组

**步骤**:
1. 启动应用程序
2. 检查日志输出,查找: `已加载通道 SiemensS7Tcp,包含 X 个设备`
3. 确认设备数量不为0

**预期结果**:
```
[INFO] 已加载通道 SiemensS7Tcp,包含 3 个设备
[INFO] 设备 PLC01 配置加载成功
[INFO] 设备 PLC02 配置加载成功
[INFO] 设备 PLC03 配置加载成功
```

#### 1.2 验证驱动特定配置提取
**目标**: 确认DeviceConfig.Options包含JSON中的驱动参数

**步骤**:
1. 在`SiemensS7Driver.ValidateConfig()`设置断点
2. 检查`config.Options`字典内容
3. 验证包含: IpAddress, Port, Rack, Slot, ScanRate, ConnectTimeout

**预期结果**:
```
config.Options:
  - IpAddress: "192.168.1.100"
  - Port: 102
  - Rack: 0
  - Slot: 1
  - ScanRate: 100
  - ConnectTimeout: 5000
  - PlcModel: "S7_1200"
```

---

### 2. 配置验证测试

#### 2.1 强类型转换和验证
**目标**: 确认`GetAndValidateDriverConfig<T>()`能正确转换和验证

**测试用例**:

**用例1**: 有效配置
```json
{
  "IpAddress": "192.168.1.100",
  "Port": 102,
  "Rack": 0,
  "Slot": 1,
  "ScanRate": 100
}
```
**预期**: 验证通过,返回`SiemensS7Config`实例

**用例2**: 无效IP地址
```json
{
  "IpAddress": "999.999.999.999",
  "Port": 102,
  "Rack": 0,
  "Slot": 1
}
```
**预期**: 抛出异常,消息包含"IpAddress字段必须是有效的IPv4地址"

**用例3**: 端口超出范围
```json
{
  "IpAddress": "192.168.1.100",
  "Port": 70000,
  "Rack": 0,
  "Slot": 1
}
```
**预期**: 抛出异常,消息包含"Port字段必须在1到65535之间"

**用例4**: 缺少必填字段
```json
{
  "Port": 102,
  "Rack": 0,
  "Slot": 1
}
```
**预期**: 抛出异常,消息包含"IpAddress字段是必填的"

#### 2.2 多重验证错误
**测试**: 提供多个无效字段
```json
{
  "IpAddress": "invalid-ip",
  "Port": -1,
  "Rack": 10,
  "Slot": 50
}
```
**预期**: 异常消息包含所有4个验证错误

---

### 3. 驱动连接测试

#### 3.1 验证强类型配置使用
**目标**: 确认SiemensS7Driver使用强类型配置值

**步骤**:
1. 在`SiemensS7Driver.ConnectAsync()`设置断点
2. 检查`driverConfig`变量类型为`SiemensS7Config`
3. 确认代码使用`driverConfig.IpAddress`而不是字典查找
4. 验证超时设置为`driverConfig.ConnectTimeout`而不是硬编码5000

**预期代码**:
```csharp
var driverConfig = _config.GetDriverConfig<SiemensS7Config>();
var ip = driverConfig.IpAddress;  // 直接属性访问
var port = driverConfig.Port;
var timeout = driverConfig.ConnectTimeout;  // 不是硬编码
```

#### 3.2 实际PLC连接测试
**前提**: 需要可访问的S7 PLC或模拟器

**步骤**:
1. 配置`SiemensS7Tcp.json`指向实际PLC
2. 启动应用程序
3. 观察连接日志

**预期结果**:
- 成功: `[INFO] 设备 PLC01 连接成功`
- 失败: 清晰的错误消息(带超时时间)

---

### 4. 热重载测试

#### 4.1 配置文件修改监控
**目标**: 验证FileSystemWatcher能检测配置变更

**步骤**:
1. 启动应用程序
2. 修改`SiemensS7Tcp.json`中的`IpAddress`
3. 保存文件

**预期**:
- 日志显示: `[INFO] 配置文件变更: SiemensS7Tcp.json`
- 设备自动重新连接

#### 4.2 无效配置热重载
**目标**: 确认运行时验证能捕获无效配置

**步骤**:
1. 运行中修改配置为无效值(如Port: 99999)
2. 保存文件

**预期**:
- 日志显示验证错误
- 设备保持旧配置,不中断服务

---

### 5. JSON Schema验证测试

#### 5.1 IDE IntelliSense
**目标**: 验证VS Code提供自动完成

**步骤**:
1. 在VS Code打开`SiemensS7Tcp.json`
2. 在Devices数组中添加新设备
3. 输入`"`后观察自动完成

**预期**: 显示可用字段(Name, Description, Enable, Code, Drive)

#### 5.2 Schema验证错误
**目标**: 验证IDE显示验证错误

**测试**:
- 将`Drive`设置为无效值(如"InvalidDriver")
- 将`Port`设置为字符串(如"102")
- 省略必填字段`Name`

**预期**: VS Code在问题面板显示错误,带波浪下划线

---

### 6. 新驱动扩展测试

#### 6.1 添加ModbusTcp驱动
**目标**: 验证扩展模式的易用性

**步骤**:
1. 参考`DeviceCommunication-AddNewDriver-QuickGuide.md`
2. 创建`ModbusTcpDriver.cs`
3. 在`ValidateConfig()`使用`config.GetAndValidateDriverConfig<ModbusTcpConfig>()`
4. 在`ConnectAsync()`使用`config.GetDriverConfig<ModbusTcpConfig>()`

**预期**: 
- 5-10分钟完成实现
- 无需修改ConfigurationLoader
- 自动获得验证功能

---

## 性能验证

### 7.1 配置加载性能
**测试**: 加载10个通道,每个通道50个设备

**指标**:
- 总加载时间 < 500ms
- 内存增长 < 10MB

### 7.2 验证性能
**测试**: 验证1000次配置对象

**指标**:
- 单次验证 < 1ms
- 无明显内存泄漏

---

## 回归测试

### 8.1 向后兼容性
**验证**: 旧版本JSON配置(如果存在)仍能加载

**重点**:
- Options字典仍然可访问
- `GetDriverConfig<T>()`不破坏现有代码
- 新旧驱动共存

### 8.2 系统集成
**验证**: 强类型配置不影响其他模块

**检查**:
- MES服务正常运行
- Http服务正常运行
- 工位设备架构正常工作

---

## 故障排除

### 常见问题

#### 问题1: Devices数组仍为空
**检查**:
1. JSON文件格式正确(`Devices`是数组)
2. 使用最新的`ConfigurationLoader.cs`
3. 检查日志错误消息

#### 问题2: 验证失败但配置看起来正确
**检查**:
1. IP地址格式(必须是点分十进制)
2. 端口范围(1-65535)
3. 所有必填字段存在

#### 问题3: Schema验证不工作
**检查**:
1. JSON文件包含`$schema`字段
2. Schema文件在`Schemas`目录
3. VS Code安装JSON插件

#### 问题4: 热重载不触发
**检查**:
1. FileSystemWatcher正常运行
2. 文件保存到正确位置
3. 检查文件权限

---

## 测试签署

| 测试类型 | 负责人 | 完成日期 | 状态 | 备注 |
|---------|--------|---------|------|-----|
| 配置加载测试 | | | ⏳ 待测 | |
| 配置验证测试 | | | ⏳ 待测 | |
| 驱动连接测试 | | | ⏳ 待测 | |
| 热重载测试 | | | ⏳ 待测 | |
| Schema测试 | | | ⏳ 待测 | |
| 新驱动扩展测试 | | | ⏳ 待测 | |
| 性能验证 | | | ⏳ 待测 | |
| 回归测试 | | | ⏳ 待测 | |

---

## 附录

### 测试数据文件位置
- 配置文件: `Configs/DeviceCommunications/Channels/`
- Schema文件: `Configs/DeviceCommunications/Schemas/`
- 日志输出: 应用程序日志目录

### 相关文档
- [实现总结](DeviceCommunication-StrongTypedConfig-Implementation.md)
- [开发者快速指南](DeviceCommunication-AddNewDriver-QuickGuide.md)
- [配置README](../src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/README.md)
- [架构索引](README-Architecture-Index.md)
