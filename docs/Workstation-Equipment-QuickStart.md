# 工位-设备架构快速入门

## 一、五分钟快速启动

### 1.1 核心概念
```
生产线 Line
  └─ 工段 Section
       └─ 工位 Workstation (业务模型)
            └─ 设备 Equipment (业务模型)
                 └─ 标签映射 EquipmentTagMapping (关联到通讯层Tag)
```

**通讯层** (低层):
- **Channel** (通讯通道): `192.168.0.100:102`
- **Device** (PLC设备): `Siemens S7-1500`
- **Tag** (标签): `DB1.DBX0.0 (心跳)`, `DB1.DBD4 (产量)`

**业务层** (高层):
- **Workstation** (工位): 生产单元,可包含多个设备
- **Equipment** (设备): 物理设备,具有类型、能力、状态
- **EquipmentTagMapping**: 业务设备与通讯标签的桥梁

---

## 二、完整工作流程

### 2.1 从PLC Schema生成标签

**步骤1**: 创建DB Schema文件 `Configs/DbSchemas/DB1_Workstation01.schema.json`

```json
{
  "DbNumber": 1,
  "Fields": [
    {
      "Name": "Workstation01_Heartbeat",
      "DataType": "Boolean",
      "Offset": 0,
      "BitIndex": 0,
      "RW": "R",
      "Description": "工位01心跳"
    },
    {
      "Name": "Workstation01_OutputCount",
      "DataType": "Int32",
      "Offset": 4,
      "RW": "R",
      "Description": "工位01产量"
    },
    {
      "Name": "Workstation01_AlarmCode",
      "DataType": "Int16",
      "Offset": 8,
      "RW": "R",
      "Description": "工位01报警码"
    }
  ]
}
```

**步骤2**: 使用 `TagGenerationService` 生成预览

```csharp
// 在应用层注入服务
public class MyViewModel
{
    private readonly ITagGenerationService _tagGenerationService;

    public async Task GenerateTagsFromSchemaAsync()
    {
        // 生成预览
        var preview = await _tagGenerationService.PreviewTagsFromDbSchemaAsync(
            dbNumber: 1,
            backupFilePath: "tags_backup.csv"
        );

        // 检查预览结果
        Console.WriteLine($"共生成 {preview.Count} 个标签:");
        foreach (var tagData in preview)
        {
            Console.WriteLine($"  - {tagData["Name"]}: {tagData["Address"]} ({tagData["DataType"]})");
        }

        // 确认后合并到正式文件
        var result = await _tagGenerationService.MergeGeneratedTagsAsync(
            preview,
            backupFilePath: "tags_backup.csv"
        );

        Console.WriteLine($"合并完成: {result}");
    }
}
```

**步骤3**: 查看生成的 `tags.csv`

```csv
ChannelName,DeviceName,TagName,Address,DataType,ArrayLength,AccessRights
Channel_PLC01,Device_S71500,Workstation01_Heartbeat,DB1.DBX0.0,Boolean,0,R
Channel_PLC01,Device_S71500,Workstation01_OutputCount,DB1.DBD4,Int32,0,R
Channel_PLC01,Device_S71500,Workstation01_AlarmCode,DB1.DBW8,Int16,0,R
```

---

### 2.2 从规则批量生成标签

```csharp
public async Task GenerateTagsByRulesAsync()
{
    var rules = new AddressRules
    {
        DbNumber = 2,
        NameTemplate = "Workstation02_Sensor_{Index}",
        StartOffset = 0,
        Stride = 4,
        Count = 10,
        DataType = "Int32",
        AccessRights = "R"
    };

    var preview = await _tagGenerationService.PreviewTagsByRulesAsync(
        rulesObj: rules,
        backupFilePath: "tags_backup.csv"
    );

    // 生成: Workstation02_Sensor_0 ~ Workstation02_Sensor_9
    // 地址: DB2.DBD0, DB2.DBD4, DB2.DBD8, ..., DB2.DBD36
}
```

---

### 2.3 创建业务模型 (工位+设备)

```csharp
// 1. 创建工位实体
var workstation01 = new Workstation
{
    Code = "WS-01",
    Name = "一号工位",
    SectionCode = "SEC-A",
    Status = WorkstationStatus.Running,
    Enabled = true
};

// 2. 创建设备实体
var bagPicker = new Equipment
{
    Code = "BPK-01",
    Name = "袋料拾取机",
    EquipmentType = EquipmentType.BagPicker,
    Capabilities = Capabilities.Heartbeat | Capabilities.AlarmReport | Capabilities.OutputCount,
    Status = EquipmentStatus.Running,
    Enabled = true
};

// 3. 添加标签映射 (将设备关联到通讯层标签)
bagPicker.AddTagMapping(new EquipmentTagMapping
{
    TagName = "Workstation01_Heartbeat",  // 对应tags.csv中的TagName
    Purpose = EquipmentTagMapping.TagPurpose.Heartbeat,
    Direction = "R"
});

bagPicker.AddTagMapping(new EquipmentTagMapping
{
    TagName = "Workstation01_OutputCount",
    Purpose = EquipmentTagMapping.TagPurpose.OutputCount,
    Direction = "R"
});

bagPicker.AddTagMapping(new EquipmentTagMapping
{
    TagName = "Workstation01_AlarmCode",
    Purpose = EquipmentTagMapping.TagPurpose.AlarmCode,
    Direction = "R"
});

// 4. 将设备添加到工位
workstation01.AddEquipment(bagPicker);

// 5. 保存到数据库
await _workstationRepository.AddAsync(workstation01);
await _unitOfWork.SaveChangesAsync();
```

---

### 2.4 使用通讯服务读写标签

```csharp
public class ProductionMonitoringService
{
    private readonly IDeviceCommunicationService _commService;
    private readonly IWorkstationRepository _wsRepository;

    public async Task MonitorWorkstationAsync(string workstationCode)
    {
        // 1. 从业务层获取工位及其设备
        var workstation = await _wsRepository.GetByCodeAsync(workstationCode);
        if (workstation == null) return;

        foreach (var equipment in workstation.Equipments)
        {
            // 2. 根据映射读取标签值
            foreach (var mapping in equipment.TagMappings)
            {
                var tagValue = await _commService.GetTagValueAsync(mapping.TagName);
                
                switch (mapping.Purpose)
                {
                    case EquipmentTagMapping.TagPurpose.Heartbeat:
                        Console.WriteLine($"{equipment.Name} 心跳: {tagValue}");
                        break;
                    case EquipmentTagMapping.TagPurpose.OutputCount:
                        Console.WriteLine($"{equipment.Name} 产量: {tagValue}");
                        break;
                    case EquipmentTagMapping.TagPurpose.AlarmCode:
                        if (Convert.ToInt32(tagValue) > 0)
                            Console.WriteLine($"{equipment.Name} 报警: {tagValue}");
                        break;
                }
            }
        }
    }

    public async Task ResetEquipmentOutputAsync(string equipmentCode)
    {
        var equipment = await _equipmentRepository.GetByCodeAsync(equipmentCode);
        var resetMapping = equipment.TagMappings
            .FirstOrDefault(m => m.Purpose == EquipmentTagMapping.TagPurpose.OutputCountReset);

        if (resetMapping != null)
        {
            await _commService.SetTagValueAsync(resetMapping.TagName, true);
            await Task.Delay(500);  // 脉冲复位
            await _commService.SetTagValueAsync(resetMapping.TagName, false);
        }
    }
}
```

---

## 三、常见场景示例

### 3.1 场景1: 新增一个工位

```csharp
// 1. 先创建Schema文件: DbSchemas/DB3_Workstation02.schema.json
// 2. 生成标签
var tags = await _tagGenerationService.PreviewTagsFromDbSchemaAsync(3, "backup.csv");
await _tagGenerationService.MergeGeneratedTagsAsync(tags, "backup.csv");

// 3. 创建工位和设备
var ws = new Workstation { Code = "WS-02", Name = "二号工位", ... };
var eq = new Equipment { Code = "PKG-01", EquipmentType = EquipmentType.PackageMachine, ... };
eq.AddTagMapping(new EquipmentTagMapping { TagName = "Workstation02_Heartbeat", Purpose = "Heartbeat" });
ws.AddEquipment(eq);
await _wsRepository.AddAsync(ws);
```

### 3.2 场景2: 批量生成测试标签

```csharp
var rules = new AddressRules
{
    DbNumber = 100,
    NameTemplate = "Test_Tag_{Index}",
    StartOffset = 0,
    Stride = 2,
    Count = 50,
    DataType = "Int16",
    AccessRights = "RW"
};

var preview = await _tagGenerationService.PreviewTagsByRulesAsync(rules, null);
await _tagGenerationService.MergeGeneratedTagsAsync(preview, null);
```

### 3.3 场景3: 从映射读取所有标签值

```csharp
public async Task<Dictionary<string, object>> GetEquipmentDataAsync(Equipment equipment)
{
    var data = new Dictionary<string, object>();
    
    foreach (var mapping in equipment.TagMappings)
    {
        var value = await _commService.GetTagValueAsync(mapping.TagName);
        data[mapping.Purpose] = value;
    }
    
    return data;
}
```

---

## 四、架构优势

### 4.1 层次分离
- **通讯层独立**: 更换PLC厂商时,只需修改Driver实现,上层业务逻辑不受影响
- **业务层解耦**: Workstation/Equipment 不依赖具体通讯协议

### 4.2 灵活性
- **多对多映射**: 一个设备可以映射多个Tag,一个Tag也可以被多个设备使用
- **动态绑定**: 通过 `TagName` 字符串动态关联,支持热配置

### 4.3 可扩展性
- **新增设备类型**: 只需扩展 `EquipmentType` 枚举
- **新增设备能力**: 只需扩展 `Capabilities` Flags枚举

---

## 五、项目文件索引

### 5.1 核心文件

| 文件路径 | 说明 |
|---------|------|
| `Domain/Entities/Workstation.cs` | 工位实体 |
| `Domain/Entities/Equipment.cs` | 设备实体 |
| `Domain/Entities/EquipmentTagMapping.cs` | 标签映射实体 |
| `Domain.Shared/Models/Equipment/EquipmentType.cs` | 设备类型枚举 |
| `Domain.Shared/Models/Equipment/Capabilities.cs` | 设备能力枚举 |
| `Infrastructure/Services/TagGenerationServiceImpl.cs` | 标签生成服务实现 |
| `Infrastructure/DeviceCommunication/Configs/ConfigurationLoader.cs` | 配置加载器 |
| `Infrastructure/DeviceCommunication/DeviceAddressing/S7AddressScanner.cs` | S7地址扫描器 |
| `Infrastructure/DeviceCommunication/Drivers/SiemensS7Driver.cs` | 西门子S7驱动 |

### 5.2 配置文件

| 文件路径 | 说明 |
|---------|------|
| `Configs/channels.csv` | 通道配置 |
| `Configs/tags.csv` | 标签配置 (生成目标) |
| `Configs/DbSchemas/*.schema.json` | DB结构Schema文件 |

### 5.3 文档

| 文档路径 | 说明 |
|---------|------|
| `docs/Workstation-Equipment-Architecture.md` | 完整架构文档 |
| `docs/Workstation-Equipment-QuickStart.md` | 本快速入门 (你正在阅读) |

---

## 六、常见问题

### Q1: 如何测试S7连接?

```csharp
var scanner = new S7AddressScanner();
var isConnected = await scanner.TestConnectionAsync("192.168.0.100", 102, 0, 1);
Console.WriteLine(isConnected ? "连接成功" : "连接失败");
```

### Q2: 标签生成后如何回滚?

生成时指定 `backupFilePath`, 系统会自动备份为 `.bak` 文件:
```csharp
await _tagGenerationService.PreviewTagsFromDbSchemaAsync(1, "tags_backup.csv");
// 生成 tags_backup.csv.bak
```

手动回滚:
```powershell
Copy-Item "Configs/tags_backup.csv.bak" "Configs/tags.csv" -Force
```

### Q3: 如何批量更新设备能力?

```csharp
var allBagPickers = await _equipmentRepository.GetByTypeAsync(EquipmentType.BagPicker);
foreach (var eq in allBagPickers)
{
    eq.UpdateCapabilities(eq.Capabilities | Capabilities.RemoteControl);
}
await _unitOfWork.SaveChangesAsync();
```

### Q4: 支持哪些S7数据类型?

- **Boolean**: `DB1.DBX0.0` (字节0,位0)
- **Int16**: `DB1.DBW2` (字偏移2)
- **Int32**: `DB1.DBD4` (双字偏移4)
- **Float**: `DB1.DBD8` (Real类型)
- **String**: `DB1.DBS12[20]` (从偏移12开始,长度20)

---

## 七、下一步

1. **阅读完整架构文档**: [Workstation-Equipment-Architecture.md](./Workstation-Equipment-Architecture.md)
2. **查看示例Schema**: [DB1.schema.json](../src/Plant01.Upper.Infrastructure/Configs/DbSchemas/DB1.schema.json)
3. **实现Repository**: 创建 `WorkstationRepository.cs`, `EquipmentRepository.cs`
4. **集成UI**: 创建工位管理、设备监控界面
5. **扩展能力**: 添加历史数据、报警推送等功能

---

**技术支持**: 查看 [README-Index.md](./README-Index.md) 获取更多文档链接
