using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 产线配置初始化服务
/// 程序启动时从 production_lines.json 加载配置到内存
/// </summary>
public class ProductionLineConfigService : BackgroundService
{
    private readonly ProductionConfigManager _configManager;
    private readonly EquipmentConfigService _equipmentConfigService;
    private readonly MultiFormatConfigLoader _configLoader;
    private readonly ILogger<ProductionLineConfigService> _logger;
    private FileSystemWatcher? _watcher;

    public ProductionLineConfigService(
        ProductionConfigManager configManager,
        EquipmentConfigService equipmentConfigService,
        MultiFormatConfigLoader configLoader,
        ILogger<ProductionLineConfigService> logger)
    {
        _configManager = configManager;
        _equipmentConfigService = equipmentConfigService;
        _configLoader = configLoader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadConfigurationAsync();

        // 设置文件监视器以支持热重载
        var configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Lines");
        if (Directory.Exists(configDir))
        {
            _watcher = new FileSystemWatcher(configDir);
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.Filter = "*.*"; // 监视所有文件
            _watcher.Changed += OnConfigChanged;
            _watcher.Created += OnConfigChanged;
            _watcher.Renamed += OnConfigChanged;
            _watcher.EnableRaisingEvents = true;
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        // 可以在此添加简单的防抖处理
        _logger.LogInformation($"[ 产线配置服务 ] 配置文件发生变更: {e.Name}，正在重新加载...");
        _ = LoadConfigurationAsync();
    }

    private async Task LoadConfigurationAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Lines");
                var mainConfigPath = Path.Combine(baseDir, "production_lines.json");
                var stationsDir = Path.Combine(baseDir, "Stations");

                // 1. 加载主拓扑结构 (JSON)
                var lineDtos = new List<ProductionLineDto>();
                if (File.Exists(mainConfigPath))
                {
                    lineDtos = _configLoader.LoadFromFile<ProductionLineDto>(mainConfigPath);
                }

                // 2. 加载额外工位 (CSV/JSON)
                var extraStations = _configLoader.LoadFromDirectory<WorkstationRefDto>(stationsDir);

                // 3. 合并并构建实体
                var productionLines = new List<ProductionLine>();
                var orphanedWorkstations = new List<Workstation>();

                // 处理主产线
                foreach (var lineDto in lineDtos)
                {
                    var line = new ProductionLine
                    {
                        Code = lineDto.Code,
                        Name = lineDto.Name,
                        Description = lineDto.Description,
                        StrategyConfigJson = lineDto.StrategyConfigJson,
                        Workstations = new List<Workstation>()
                    };

                    // 添加 JSON 中定义的工位
                    foreach (var wsDto in lineDto.Workstations)
                    {
                        var refDto = new WorkstationRefDto
                        {
                            Code = wsDto.Code,
                            Name = wsDto.Name,
                            Type = wsDto.Type ?? string.Empty,
                            Sequence = wsDto.Sequence,
                            EquipmentRefs = wsDto.EquipmentRefs,
                            LineCode = line.Code
                        };
                        AddWorkstationToLine(line, refDto);
                    }

                    productionLines.Add(line);
                }

                // 处理额外工位 (来自 CSV/JSON 文件)
                foreach (var wsDto in extraStations)
                {
                    // 处理管道分隔的设备引用
                    if (!string.IsNullOrEmpty(wsDto.EquipmentRefsStr))
                    {
                        var refs = wsDto.EquipmentRefsStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        wsDto.EquipmentRefs.AddRange(refs);
                    }

                    // 查找父产线
                    var parentLine = productionLines.FirstOrDefault(l => l.Code == wsDto.LineCode);
                    if (parentLine != null)
                    {
                        AddWorkstationToLine(parentLine, wsDto);
                    }
                    else
                    {
                        // 创建孤立工位用于报告
                        var ws = CreateWorkstationEntity(null, wsDto);
                        orphanedWorkstations.Add(ws);
                        _logger.LogWarning($"[ 产线配置服务 ] 发现孤立工位: {wsDto.Code}。未找到对应的产线代码 '{wsDto.LineCode}'。");
                    }
                }

                // 加载到内存
                _configManager.LoadFromConfig(productionLines);

                if (orphanedWorkstations.Any())
                {
                    _logger.LogWarning($"[ 产线配置服务 ] 加载配置时发现 {orphanedWorkstations.Count} 个孤立工位。");
                }

                _logger.LogInformation($"[ 产线配置服务 ] 产线配置已成功加载到内存: {_configManager.GetConfigSummary()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ 产线配置服务 ] 加载产线配置失败");
            }
        });
    }

    private void AddWorkstationToLine(ProductionLine line, WorkstationRefDto wsDto)
    {
        // 检查是否有重复工位
        if (line.Workstations.Any(w => w.Code == wsDto.Code))
        {
            _logger.LogWarning($"[ 产线配置服务 ] 产线 '{line.Code}' 中存在重复工位代码 '{wsDto.Code}'，已跳过。");
            return;
        }

        var workstation = CreateWorkstationEntity(line, wsDto);
        line.Workstations.Add(workstation);
    }

    private Workstation CreateWorkstationEntity(ProductionLine? line, WorkstationRefDto wsDto)
    {
        // 1. 通过引用获取设备 (JSON/管道分隔)
        var equipments = _equipmentConfigService.GetEquipmentsByRefs(wsDto.EquipmentRefs);

        // 2. 通过父关联获取设备 (CSV 工位代码)
        // 先获取 DTO，提取 Code，再通过 GetEquipmentsByRefs 统一创建完整的实体（包含标签、能力等）
        var childEquipmentDtos = _equipmentConfigService.GetEquipmentsByStationCode(wsDto.Code);
        var childEquipments = _equipmentConfigService.GetEquipmentsByRefs(
            childEquipmentDtos.Select(d => d.Code).ToList()
        );

        // 3. 合并设备 (按 Code 去重)
        var allEquipments = equipments.Union(childEquipments)
            .GroupBy(e => e.Code)
            .Select(g => g.First())
            .ToList();

        var workstation = new Workstation
        {
            Code = wsDto.Code,
            Name = wsDto.Name,
            Type = wsDto.Type,
            Sequence = wsDto.Sequence,
            ProductionLine = line!, // 对于孤立工位可为 null
            Equipments = allEquipments
        };

        foreach (var eq in allEquipments)
        {
            eq.Workstation = workstation;
        }

        return workstation;
    }

}

