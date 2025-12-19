# 设备通信配置指南

## 概述

本目录包含设备通信层的配置文件,用于定义通道、设备和标签的配置。

## 目录结构

```
DeviceCommunications/
├── Channels/           # 通道配置文件 (JSON)
│   ├── SiemensS7Tcp.json
│   ├── ModbusTcp.json
│   └── Simulation.json
├── Schemas/            # JSON Schema 验证文件
│   └── channel-config.schema.json
└── README.md           # 本文件
```

## 配置文件格式

### Channel 配置结构

```json
{
  "$schema": "../Schemas/channel-config.schema.json",
  "Code": "通道代码",
  "Name": "通道名称",
  "Enable": true,
  "Description": "通道描述",
  "Drive": "驱动类型",
  "DriveModel": "驱动型号",
  "Devices": [...]
}
```

### 支持的驱动类型

#### 1. SiemensS7 (西门子 S7 PLC)

**配置参数:**

| 参数名 | 类型 | 必需 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `IpAddress` | string | ✅ | - | PLC IP 地址 |
| `Port` | integer | ✅ | 102 | TCP 端口 (1-65535) |
| `Rack` | integer | ❌ | 0 | 机架号 (0-7) |
| `Slot` | integer | ❌ | 1 | 插槽号 (0-31) |
| `ScanRate` | integer | ❌ | 100 | 扫描速率/毫秒 (10-10000) |
| `ConnectTimeout` | integer | ❌ | 5000 | 连接超时/毫秒 (1000-60000) |
| `PlcModel` | string | ❌ | S7_1200 | PLC型号: S7_200/S7_300/S7_400/S7_1200/S7_1500 |

**示例:**

```json
{
  "Name": "PLC01",
  "Enable": true,
  "Description": "生产线1 PLC",
  "IpAddress": "192.168.1.100",
  "Port": 102,
  "Rack": 0,
  "Slot": 1,
  "ScanRate": 100,
  "ConnectTimeout": 5000,
  "PlcModel": "S7_1200"
}
```

#### 2. ModbusTcp (Modbus TCP)

**配置参数:**

| 参数名 | 类型 | 必需 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `IpAddress` | string | ✅ | - | Modbus服务器IP地址 |
| `Port` | integer | ✅ | 502 | TCP 端口 (1-65535) |
| `SlaveId` | integer | ❌ | 1 | 从站地址 (1-247) |
| `ScanRate` | integer | ❌ | 100 | 扫描速率/毫秒 (10-10000) |
| `ConnectTimeout` | integer | ❌ | 5000 | 连接超时/毫秒 (1000-60000) |

**示例:**

```json
{
  "Name": "Modbus01",
  "Enable": true,
  "Description": "温度传感器",
  "IpAddress": "192.168.1.200",
  "Port": 502,
  "SlaveId": 1,
  "ScanRate": 200,
  "ConnectTimeout": 5000
}
```

#### 3. Simulation (仿真驱动)

**配置参数:**

| 参数名 | 类型 | 必需 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `SimulationDelay` | integer | ❌ | 50 | 仿真延迟/毫秒 |
| `RandomSeed` | integer | ❌ | 0 | 随机数种子 (0=使用时间戳) |

**示例:**

```json
{
  "Name": "Sim01",
  "Enable": true,
  "Description": "仿真设备",
  "SimulationDelay": 50,
  "RandomSeed": 12345
}
```

## 配置验证

配置文件在加载时会自动进行验证:

1. **JSON Schema 验证**: 确保文件结构符合 schema 定义
2. **DataAnnotations 验证**: 验证参数范围和格式
3. **驱动特定验证**: 每个驱动的 `ValidateConfig()` 方法

## 配置热重载

系统支持配置文件热重载。修改配置文件后,系统会自动:

1. 停止现有通道
2. 清除标签缓存
3. 重新加载配置
4. 重新初始化通道和设备

## 最佳实践

1. **使用 `$schema` 引用**: 启用 IDE 智能提示和验证
2. **合理设置扫描速率**: 根据实际需求平衡性能和实时性
3. **添加详细描述**: 便于运维人员理解配置
4. **备份配置文件**: 修改前先备份原文件
5. **测试连接参数**: 使用工具验证 IP/端口的可达性

## 故障排查

### 常见错误

1. **"IpAddress is required"**: 检查是否正确配置 `IpAddress` 字段
2. **"Port must be between 1-65535"**: 端口号超出范围
3. **"S7 连接失败"**: 检查网络连通性、IP地址、Rack/Slot 配置

### 日志查看

系统日志位于 `logs/` 目录,包含详细的配置加载和设备连接信息。

## 参考资料

- [项目架构文档](../../../docs/Project-Summary-and-Guidelines.md)
- [设备通信架构](../../../docs/Workstation-Equipment-Architecture.md)
