using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

/// <summary>
/// PLC 调试 ViewModel
/// </summary>
public partial class PlcDebugViewModel : ObservableObject
{
    private readonly ITagGenerationService _tagGenerationService;
    private readonly ILogger<PlcDebugViewModel> _logger;

    #region PLC Debug - S7 Connection

    [ObservableProperty]
    private string _s7IpAddress = "10.168.10.1";

    [ObservableProperty]
    private int _s7Port = 102;

    [ObservableProperty]
    private int _s7Rack = 0;

    [ObservableProperty]
    private int _s7Slot = 1;

    [ObservableProperty]
    private string _connectionStatus = "未测试";

    #endregion

    #region PLC Debug - Tag Generation

    // 规则参数
    [ObservableProperty]
    private int _ruleDbNumber = 1;

    [ObservableProperty]
    private string _ruleNameTemplate = "Tag_{Index}";

    [ObservableProperty]
    private int _ruleStartOffset = 0;

    [ObservableProperty]
    private int _ruleCount = 10;

    [ObservableProperty]
    private int _ruleStride = 2; // 默认 Int16 步长为 2

    [ObservableProperty]
    private string _ruleDataType = "Int16"; // 可选: Int16, Int32, Float, Boolean

    // Schema 参数
    [ObservableProperty]
    private int _schemaDbNumber = 1;

    // 预览结果
    [ObservableProperty]
    private ObservableCollection<object> _previewTags = new();

    // TIA DB 文件内容
    [ObservableProperty]
    private string _tiaDbContent = "";

    #endregion

    public PlcDebugViewModel(
        ITagGenerationService tagGenerationService,
        ILogger<PlcDebugViewModel> logger)
    {
        _tagGenerationService = tagGenerationService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task TestS7ConnectionAsync()
    {
        ConnectionStatus = "正在连接...";
        try
        {
            var result = await _tagGenerationService.TestS7ConnectionAsync(S7IpAddress, S7Port, S7Rack, S7Slot);
            ConnectionStatus = result ? "连接成功" : "连接失败";
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"错误: {ex.Message}";
            _logger.LogError(ex, "S7 Connection Test Failed");
        }
    }

    [RelayCommand]
    private void PreviewTagsFromRules()
    {
        try
        {
            var rules = new AddressRules
            {
                DbNumber = RuleDbNumber,
                NameTemplate = RuleNameTemplate,
                StartOffset = RuleStartOffset,
                Count = RuleCount,
                Stride = RuleStride,
                DataType = RuleDataType
            };

            var tags = _tagGenerationService.PreviewFromRules(rules);
            PreviewTags = new ObservableCollection<object>(tags);
            _logger.LogInformation("Previewed {Count} tags from rules", tags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Preview Tags From Rules Failed");
        }
    }

    [RelayCommand]
    private void GenerateTagsFromRules()
    {
        try
        {
            var rules = new AddressRules
            {
                DbNumber = RuleDbNumber,
                NameTemplate = RuleNameTemplate,
                StartOffset = RuleStartOffset,
                Count = RuleCount,
                Stride = RuleStride,
                DataType = RuleDataType
            };

            var success = _tagGenerationService.GenerateAndMergeFromRules(rules);
            if (success)
            {
                _logger.LogInformation("Successfully generated tags from rules");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate Tags From Rules Failed");
        }
    }

    [RelayCommand]
    private void PreviewTagsFromSchema()
    {
        try
        {
            var tags = _tagGenerationService.PreviewFromDbSchema(SchemaDbNumber);
            PreviewTags = new ObservableCollection<object>(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Preview Tags From Schema Failed");
        }
    }

    [RelayCommand]
    private void GenerateTagsFromSchema()
    {
        try
        {
            var success = _tagGenerationService.GenerateAndMergeFromDbSchema(SchemaDbNumber);
            if (success)
            {
                _logger.LogInformation("Successfully generated tags from schema");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate Tags From Schema Failed");
        }
    }

    [RelayCommand]
    private void PreviewTagsFromTiaDb()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TiaDbContent))
            {
                // 可以添加提示
                return;
            }
            var tags = _tagGenerationService.PreviewFromTiaDbFile(TiaDbContent);
            PreviewTags = new ObservableCollection<object>(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Preview Tags From TIA DB Failed");
        }
    }

    [RelayCommand]
    private void GenerateTagsFromTiaDb()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TiaDbContent)) return;

            var success = _tagGenerationService.GenerateAndMergeFromTiaDbFile(TiaDbContent);
            if (success)
            {
                _logger.LogInformation("Successfully generated tags from TIA DB");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate Tags From TIA DB Failed");
        }
    }
}
