# 快速参考：如何添加新驱动的强类型配置

## 添加新驱动配置的 5 个步骤

### 步骤 1: 创建驱动配置类

**位置**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/DriverConfigs/YourDriverConfig.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

/// <summary>
/// Your Driver 驱动配置
/// </summary>
public class YourDriverConfig
{
    /// <summary>
    /// 参数说明 (必需/可选)
    /// </summary>
    [Required(ErrorMessage = "参数名 是必需的")]
    [Range(最小值, 最大值, ErrorMessage = "参数名 必须在 x-y 之间")]
    public string YourParameter { get; set; } = "默认值";
    
    // 添加更多参数...
}
```

**常用验证特性**:
- `[Required]` - 必需字段
- `[Range(min, max)]` - 数值范围
- `[RegularExpression(@"pattern")]` - 正则表达式
- `[StringLength(max)]` - 字符串长度
- `[EmailAddress]` - 邮箱格式
- `[Url]` - URL格式

---

### 步骤 2: 更新驱动实现

**文件**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/YourDriver.cs`

```csharp
// 添加 using
using Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;
using System.ComponentModel.DataAnnotations;

public class YourDriver : IDriver
{
    // 1. 更新 ValidateConfig
    public void ValidateConfig(DeviceConfig config)
    {
        // 自动验证所有 DataAnnotations 约束
        var driverConfig = config.GetAndValidateDriverConfig<YourDriverConfig>();
        
        // 可选: 添加额外的业务验证
        if (driverConfig.SomeParam < driverConfig.OtherParam)
            throw new ArgumentException("SomeParam 必须大于 OtherParam");
    }

    // 2. 更新 ConnectAsync (或其他使用配置的方法)
    public Task ConnectAsync()
    {
        if (_config == null) throw new InvalidOperationException("驱动程序未初始化");

        // 使用强类型配置
        var driverConfig = _config.GetDriverConfig<YourDriverConfig>();
        
        // 直接访问属性,无需类型转换
        _client = new YourClient(driverConfig.Host, driverConfig.Port)
        {
            Timeout = driverConfig.Timeout
        };
        
        // ...
    }
}
```

---

### 步骤 3: 更新 JSON Schema

**文件**: `src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/Schemas/channel-config.schema.json`

```json
{
  "properties": {
    "Drive": {
      "enum": ["SiemensS7", "ModbusTcp", "Simulation", "YourDriver"]  // 添加驱动名
    }
  },
  "$defs": {
    "device": {
      "allOf": [
        // ... 现有驱动 ...
        {
          "if": {
            "properties": {
              "Drive": { "const": "YourDriver" }
            }
          },
          "then": {
            "$ref": "#/$defs/yourDriverDevice"
          }
        }
      ]
    },
    "yourDriverDevice": {
      "type": "object",
      "required": ["Host", "Port"],  // 必需字段
      "properties": {
        "Host": {
          "type": "string",
          "description": "服务器地址",
          "pattern": "^[a-zA-Z0-9.-]+$"
        },
        "Port": {
          "type": "integer",
          "description": "端口",
          "default": 8080,
          "minimum": 1,
          "maximum": 65535
        },
        "Timeout": {
          "type": "integer",
          "description": "超时时间(毫秒)",
          "default": 5000,
          "minimum": 1000,
          "maximum": 60000
        }
      }
    }
  }
}
```

---

### 步骤 4: 创建配置文件示例

**文件**: `src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/Channels/YourDriver.json`

```json
{
  "$schema": "../Schemas/channel-config.schema.json",
  "Code": "YourDriverChannel",
  "Name": "Your Driver 通道",
  "Enable": true,
  "Description": "Your Driver 说明",
  "Drive": "YourDriver",
  "DriveModel": "V1.0",
  "Devices": [
    {
      "Name": "Device01",
      "Description": "设备1",
      "Enable": true,
      "Host": "192.168.1.100",
      "Port": 8080,
      "Timeout": 5000
    }
  ]
}
```

---

### 步骤 5: 更新 DriverFactory

**文件**: `src/Plant01.Upper.Infrastructure/DeviceCommunication/Drivers/DriverFactory.cs`

```csharp
public IDriver CreateDriver(string driverName)
{
    if (string.Equals(driverName, "SiemensS7", StringComparison.OrdinalIgnoreCase))
    {
        return new SiemensS7Driver(); 
    }
    else if (string.Equals(driverName, "YourDriver", StringComparison.OrdinalIgnoreCase))
    {
        return new YourDriver();  // 添加你的驱动
    }
    // ...
    
    return new SimulationDriver();
}
```

---

## 完整示例：添加 MQTT 驱动

### 1. MqttConfig.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

public class MqttConfig
{
    [Required(ErrorMessage = "Broker 地址是必需的")]
    public string Broker { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "Port 必须在 1-65535 之间")]
    public int Port { get; set; } = 1883;

    public string ClientId { get; set; } = "Plant01Client";

    public string? Username { get; set; }

    public string? Password { get; set; }

    [Range(0, 2, ErrorMessage = "QoS 必须在 0-2 之间")]
    public int QosLevel { get; set; } = 1;
}
```

### 2. MqttDriver.cs

```csharp
using Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

public class MqttDriver : IDriver
{
    public void ValidateConfig(DeviceConfig config)
    {
        var mqttConfig = config.GetAndValidateDriverConfig<MqttConfig>();
    }

    public Task ConnectAsync()
    {
        var mqttConfig = _config.GetDriverConfig<MqttConfig>();
        
        _client = new MqttFactory().CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttConfig.Broker, mqttConfig.Port)
            .WithClientId(mqttConfig.ClientId)
            .WithCredentials(mqttConfig.Username, mqttConfig.Password)
            .Build();
            
        await _client.ConnectAsync(options);
        return Task.CompletedTask;
    }
}
```

### 3. Schema 片段

```json
{
  "$defs": {
    "mqttDevice": {
      "type": "object",
      "required": ["Broker", "Port"],
      "properties": {
        "Broker": {
          "type": "string",
          "description": "MQTT Broker 地址"
        },
        "Port": {
          "type": "integer",
          "default": 1883,
          "minimum": 1,
          "maximum": 65535
        },
        "ClientId": {
          "type": "string",
          "default": "Plant01Client"
        },
        "Username": { "type": "string" },
        "Password": { "type": "string" },
        "QosLevel": {
          "type": "integer",
          "enum": [0, 1, 2],
          "default": 1
        }
      }
    }
  }
}
```

### 4. 配置文件

```json
{
  "$schema": "../Schemas/channel-config.schema.json",
  "Code": "MqttChannel",
  "Name": "MQTT 通道",
  "Enable": true,
  "Drive": "Mqtt",
  "Devices": [
    {
      "Name": "MqttBroker01",
      "Enable": true,
      "Broker": "mqtt.example.com",
      "Port": 1883,
      "ClientId": "Plant01_Device01",
      "Username": "user",
      "Password": "pass",
      "QosLevel": 1
    }
  ]
}
```

---

## 常见问题 FAQ

### Q: 配置类放在哪个命名空间?
**A**: `Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs`

### Q: 如何处理可选参数?
**A**: 不添加 `[Required]` 特性,并设置默认值:
```csharp
public int OptionalParam { get; set; } = 默认值;
```

### Q: 如何验证嵌套对象?
**A**: 使用 `[ValidateNested]` (需要 FluentValidation) 或手动验证。

### Q: 配置类能包含复杂类型吗?
**A**: 可以,JSON 会自动反序列化,但建议保持扁平结构。

### Q: 如何调试配置加载?
**A**: 查看日志,ConfigurationLoader 会输出:
```
已加载通道 {Channel},包含 {DeviceCount} 个设备
```

### Q: 验证失败怎么办?
**A**: `GetAndValidateDriverConfig<T>()` 会抛出 `ArgumentException`,包含所有验证错误。

---

## 最佳实践

1. **✅ 使用描述性参数名**: `IpAddress` 而非 `IP`
2. **✅ 添加 XML 注释**: 说明参数用途和范围
3. **✅ 设置合理默认值**: 常用场景无需配置
4. **✅ 使用验证特性**: 在配置类而非驱动中验证
5. **✅ 更新 Schema**: 保持 JSON Schema 与配置类同步
6. **✅ 添加配置示例**: 在 README 中提供示例

---

## 扩展属性支持 (ExtendedProperties)

从 v2.0 开始，`CommunicationTag` 支持扩展属性，允许驱动存储特定的额外字段。

### 为什么需要扩展属性？

不同驱动可能需要额外的标签配置：
- **Modbus**: 需要 `StationId`（从站地址）、`FunctionCode`（功能码）
- **Siemens S7**: 需要 `DbNumber`、`Offset`、`BitOffset`
- **OPC UA**: 需要 `NodeId`、`NamespaceIndex`

### 使用方法

#### 1. 在驱动中读取扩展属性

```csharp
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
{
    foreach (var tagObj in tags)
    {
        var tag = tagObj as CommunicationTag;
        
        // 使用扩展方法获取驱动特定属性
        var stationId = tag.GetModbusStationId(defaultValue: 1);
        var functionCode = tag.GetModbusFunctionCode(defaultValue: 3);
        
        // 使用属性进行通信
        _client.Station = stationId;
        var result = _client.Read(tag.Address, functionCode);
        // ...
    }
}
```

#### 2. 在配置文件中设置扩展属性

```json
{
  "Tags": [
    {
      "Name": "Temperature",
      "Address": "40001",
      "DataType": "Int16",
      "ExtendedProperties": {
        "ModbusStationId": 1,
        "ModbusFunctionCode": 3
      }
    }
  ]
}
```

#### 3. 验证扩展属性

```csharp
public void ValidateConfig(DeviceConfig config)
{
    var driverConfig = config.GetAndValidateDriverConfig<ModbusTcpConfig>();
    
    // 验证标签的扩展属性
    foreach (var tag in config.Tags)
    {
        if (tag.ExtendedProperties?.ContainsKey("ModbusStationId") == true)
        {
            var stationId = Convert.ToByte(tag.ExtendedProperties["ModbusStationId"]);
            if (stationId < 1 || stationId > 247)
            {
                throw new ValidationException($"标签 {tag.Name} 的 ModbusStationId 必须在 1-247 范围内");
            }
        }
    }
}
```

### 内置扩展方法

#### Modbus 驱动
- `GetModbusStationId()` / `SetModbusStationId()`
- `GetModbusFunctionCode()` / `SetModbusFunctionCode()`

#### Siemens S7 驱动
- `GetS7DbNumber()` / `SetS7DbNumber()`
- `GetS7Offset()` / `SetS7Offset()`
- `GetS7BitOffset()` / `SetS7BitOffset()`

#### OPC UA 驱动
- `GetOpcUaNodeId()` / `SetOpcUaNodeId()`
- `GetOpcUaNamespaceIndex()` / `SetOpcUaNamespaceIndex()`

### 添加自定义扩展方法

在 `CommunicationTagExtensions.cs` 中添加：

```csharp
public static class CommunicationTagExtensions
{
    #region YourDriver 特定扩展属性
    
    public static string GetYourDriverProperty(this CommunicationTag tag, string defaultValue = "")
    {
        return tag.GetExtendedProperty("YourDriverProperty", defaultValue);
    }
    
    public static void SetYourDriverProperty(this CommunicationTag tag, string value)
    {
        tag.SetExtendedProperty("YourDriverProperty", value);
    }
    
    #endregion
}
```

**详细文档**: `docs/DeviceCommunication-ExtendedProperties-Guide.md`

---

## 参考资料

- **扩展属性指南**: `docs/DeviceCommunication-ExtendedProperties-Guide.md` ⭐
- **完整实施总结**: `docs/DeviceCommunication-StrongTypedConfig-Implementation.md`
- **配置文档**: `src/Plant01.Upper.Infrastructure/Configs/DeviceCommunications/README.md`
- **架构指南**: `docs/Project-Summary-and-Guidelines.md`
- **DataAnnotations**: https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations
- **JSON Schema**: https://json-schema.org/
