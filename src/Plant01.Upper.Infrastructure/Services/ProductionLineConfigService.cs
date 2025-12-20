using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Infrastructure.Configs.Models;

using System.Text.Json;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 产线配置初始化服务
/// 程序启动时从 production_lines.json 加载配置到内存
/// </summary>
public class ProductionLineConfigService : BackgroundService
{
    private readonly ProductionConfigManager _configManager;
    private readonly EquipmentConfigService _equipmentConfigService;
    private readonly ILogger<ProductionLineConfigService> _logger;

    public ProductionLineConfigService(
        ProductionConfigManager configManager,
        EquipmentConfigService equipmentConfigService,
        ILogger<ProductionLineConfigService> logger)
    {
        _configManager = configManager;
        _equipmentConfigService = equipmentConfigService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Lines", "production_lines.json");

                if (!File.Exists(configPath))
                {
                    _logger.LogWarning($"产线配置文件不存在: {configPath}");
                    return;
                }

                var json = File.ReadAllText(configPath);
                var lineDtos = JsonSerializer.Deserialize<List<ProductionLineDto>>(json)
                    ?? new List<ProductionLineDto>();

                // 将 DTO 转换为实体对象，并使用 EquipmentConfigService 加载设备
                var productionLines = new List<ProductionLine>();

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

                    foreach (var workstationDto in lineDto.Workstations)
                    {
                        // 通过 EquipmentConfigService 加载工位的设备
                        var equipments = _equipmentConfigService.GetEquipmentsByRefs(workstationDto.EquipmentRefs);

                        var workstation = new Workstation
                        {
                            Code = workstationDto.Code,
                            Name = workstationDto.Name,
                            Type = workstationDto.Type,
                            Sequence = workstationDto.Sequence,
                            ProductionLine = line,
                            Equipments = equipments
                        };

                        // 设置设备的工位关联
                        foreach (var equipment in equipments)
                        {
                            equipment.Workstation = workstation;
                        }

                        line.Workstations.Add(workstation);
                    }

                    productionLines.Add(line);
                }

                // 加载到内存
                _configManager.LoadFromConfig(productionLines);
                _logger.LogInformation($"产线配置已成功加载到内存: {_configManager.GetConfigSummary()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载产线配置失败");
                throw;
            }
        }, stoppingToken);
    }
}

