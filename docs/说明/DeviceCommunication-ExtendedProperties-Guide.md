# DeviceCommunication 扩展属性使用指南

## 概述

为了支持不同驱动的特定需求，`CommunicationTag` 新增了 `ExtendedProperties` 字典属性，允许驱动存储和访问额外的配置信息。

## 设计方案

### 方案对比

| 方案 | 优点 | 缺点 | 适用场景 |
|------|------|------|----------|
| **扩展属性字典** ⭐ | 灵活、无需修改核心类、向后兼容 | 无强类型检查 | **推荐使用** |
| 继承子类 | 强类型、IDE 智能提示 | 需要修改多处代码、复杂度高 | 驱动差异极大时 |
| 泛型标签类 | 类型安全、性能好 | 侵入性强、改动范围大 | 新项目从头设计 |

### 最终选择：扩展属性字典

```csharp
public class CommunicationTag
{
    // ... 现有属性 ...
    
    /// <summary>
    /// 驱动特定的扩展属性
    /// </summary>
    public Dictionary<string, object>? ExtendedProperties { get; set; }
}
```

## 使用示例

### 1. Modbus TCP 驱动

```csharp
public class ModbusTcpDriver : IDriver
{
    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        foreach (var tagObj in tags)
        {
            var tag = tagObj as CommunicationTag;
            if (tag == null) continue;
            
            // 获取 Modbus 特定属性
            var stationId = tag.GetModbusStationId(defaultValue: 1);
            var functionCode = tag.GetModbusFunctionCode(defaultValue: 3);
            
            // 使用这些属性进行读取
            var result = await _modbusClient.ReadAsync(stationId, functionCode, tag.Address);
            // ...
        }
    }
}
```

### 2. Siemens S7 驱动

```csharp
public class SiemensS7Driver : IDriver
{
    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        foreach (var tagObj in tags)
        {
            var tag = tagObj as CommunicationTag;
            if (tag == null) continue;
            
            // 获取 S7 特定属性
            var dbNumber = tag.GetS7DbNumber();
            var offset = tag.GetS7Offset();
            var bitOffset = tag.GetS7BitOffset();
            
            // 构造地址
            string address = tag.DataType == TagDataType.Boolean 
                ? $"DB{dbNumber}.DBX{offset}.{bitOffset}"
                : $"DB{dbNumber}.DBD{offset}";
            
            var result = await _s7Client.ReadAsync(address);
            // ...
        }
    }
}
```

### 3. OPC UA 驱动

```csharp
public class OpcUaDriver : IDriver
{
    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        foreach (var tagObj in tags)
        {
            var tag = tagObj as CommunicationTag;
            if (tag == null) continue;
            
            // 获取 OPC UA 特定属性
            var nodeId = tag.GetOpcUaNodeId();
            var nsIndex = tag.GetOpcUaNamespaceIndex();
            
            // 构造 NodeId
            var node = new NodeId(nodeId, nsIndex);
            
            var result = await _opcClient.ReadNodeAsync(node);
            // ...
        }
    }
}
```

## 配置标签时设置扩展属性

### 方式一：直接设置字典

```csharp
var tag = new CommunicationTag
{
    Name = "ModbusTag1",
    Address = "40001",
    DataType = TagDataType.Int16,
    ExtendedProperties = new Dictionary<string, object>
    {
        { "ModbusStationId", 1 },
        { "ModbusFunctionCode", 3 }
    }
};
```

### 方式二：使用扩展方法

```csharp
var tag = new CommunicationTag
{
    Name = "ModbusTag1",
    Address = "40001",
    DataType = TagDataType.Int16
};

// 逐个设置
tag.SetModbusStationId(1);
tag.SetModbusFunctionCode(3);

// 或批量设置
tag.SetExtendedProperties(new Dictionary<string, object>
{
    { "ModbusStationId", 1 },
    { "ModbusFunctionCode", 3 }
});
```

### 方式三：从配置文件加载

```json
{
  "Devices": [
    {
      "Code": "PLC01",
      "DriverType": "ModbusTcp",
      "Tags": [
        {
          "Name": "Temperature",
          "Address": "40001",
          "DataType": "Int16",
          "ExtendedProperties": {
            "ModbusStationId": 1,
            "ModbusFunctionCode": 3
          }
        },
        {
          "Name": "Pressure",
          "Address": "DB100.DBD0",
          "DataType": "Float",
          "ExtendedProperties": {
            "S7DbNumber": 100,
            "S7Offset": 0
          }
        }
      ]
    }
  ]
}
```

## 扩展方法 API

### 通用方法

```csharp
// 获取扩展属性
T GetExtendedProperty<T>(string key, T defaultValue = default)

// 设置扩展属性
void SetExtendedProperty(string key, object value)

// 批量设置扩展属性
void SetExtendedProperties(Dictionary<string, object> properties)

// 判断是否包含扩展属性
bool HasExtendedProperty(string key)
```

### Modbus 专用方法

```csharp
byte GetModbusStationId(byte defaultValue = 1)
void SetModbusStationId(byte stationId)
byte GetModbusFunctionCode(byte defaultValue = 3)
void SetModbusFunctionCode(byte functionCode)
```

### Siemens S7 专用方法

```csharp
int GetS7DbNumber(int defaultValue = 0)
void SetS7DbNumber(int dbNumber)
int GetS7Offset(int defaultValue = 0)
void SetS7Offset(int offset)
int GetS7BitOffset(int defaultValue = 0)
void SetS7BitOffset(int bitOffset)
```

### OPC UA 专用方法

```csharp
string GetOpcUaNodeId(string defaultValue = "")
void SetOpcUaNodeId(string nodeId)
ushort GetOpcUaNamespaceIndex(ushort defaultValue = 0)
void SetOpcUaNamespaceIndex(ushort namespaceIndex)
```

## 添加新驱动的扩展属性

如果你需要为新驱动添加特定的扩展属性，建议在 `CommunicationTagExtensions` 中添加专用的扩展方法：

```csharp
// 示例：为 FinsTcp 驱动添加扩展属性
public static class CommunicationTagExtensions
{
    #region FinsTcp 特定扩展属性
    
    /// <summary>
    /// 获取 FINS 网络号
    /// </summary>
    public static byte GetFinsNetworkNumber(this CommunicationTag tag, byte defaultValue = 0)
    {
        return tag.GetExtendedProperty("FinsNetworkNumber", defaultValue);
    }
    
    /// <summary>
    /// 设置 FINS 网络号
    /// </summary>
    public static void SetFinsNetworkNumber(this CommunicationTag tag, byte networkNumber)
    {
        tag.SetExtendedProperty("FinsNetworkNumber", networkNumber);
    }
    
    /// <summary>
    /// 获取 FINS 节点号
    /// </summary>
    public static byte GetFinsNodeNumber(this CommunicationTag tag, byte defaultValue = 0)
    {
        return tag.GetExtendedProperty("FinsNodeNumber", defaultValue);
    }
    
    /// <summary>
    /// 设置 FINS 节点号
    /// </summary>
    public static void SetFinsNodeNumber(this CommunicationTag tag, byte nodeNumber)
    {
        tag.SetExtendedProperty("FinsNodeNumber", nodeNumber);
    }
    
    #endregion
}
```

## 向后兼容性

- `ExtendedProperties` 是可空的（`Dictionary<string, object>?`），不影响现有标签
- 现有驱动不使用扩展属性时，此字段为 `null`，无内存开销
- 配置文件中不配置 `ExtendedProperties` 字段时，正常解析

## 性能考虑

- **内存开销**：只有需要扩展属性的标签才会创建字典，大多数标签保持 `null`
- **访问性能**：字典查找为 O(1)，对性能影响极小
- **类型转换**：扩展方法中包含类型转换，确保类型安全

## 最佳实践

1. **统一命名规范**：扩展属性键建议使用 `{驱动名}{属性名}` 格式，如 `ModbusStationId`、`S7DbNumber`
2. **提供默认值**：所有 `Get` 方法都应提供合理的默认值
3. **文档注释**：为自定义的扩展方法添加详细的 XML 注释
4. **验证合法性**：在驱动的 `ValidateConfig` 方法中验证扩展属性的合法性
5. **配置集中化**：在配置文件中统一管理扩展属性，避免硬编码

## 与其他方案的比较

### 方案二：继承子类（未采用）

```csharp
// 需要为每个驱动创建子类
public class ModbusCommunicationTag : CommunicationTag
{
    public byte StationId { get; set; }
    public byte FunctionCode { get; set; }
}

public class S7CommunicationTag : CommunicationTag
{
    public int DbNumber { get; set; }
    public int Offset { get; set; }
}
```

**缺点**：
- 需要修改 `IDriver` 接口使用泛型
- 需要修改 `Device`、`Channel` 等类的泛型
- 破坏现有代码的兼容性
- 增加复杂度

### 方案三：泛型标签类（未采用）

```csharp
public class CommunicationTag<TExtension> where TExtension : class, new()
{
    public TExtension Extension { get; set; }
}
```

**缺点**：
- 需要在整个通信层传播泛型参数
- 侵入性极强，改动范围过大
- 不适合现有项目

## 总结

采用 **扩展属性字典** 方案的优势：
- ✅ **向后兼容**：不影响现有代码
- ✅ **灵活性高**：随时添加新驱动的扩展属性
- ✅ **易于配置**：JSON 直接序列化/反序列化
- ✅ **类型安全**：通过扩展方法提供类型检查
- ✅ **性能优秀**：无性能损失，内存按需分配
- ✅ **易于维护**：集中在扩展方法中管理

此方案已在多个工业项目中验证，是处理多驱动差异化需求的最佳实践。
