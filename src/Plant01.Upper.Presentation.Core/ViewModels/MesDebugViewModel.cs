using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Plant01.Upper.Presentation.Core.ViewModels;

/// <summary>
/// MES 接口调试 ViewModel
/// </summary>
public partial class MesDebugViewModel : ObservableObject
{
    private readonly IMesService _mesService;
    private readonly IMesWebApi _mesWebApi;
    private readonly ILogger<MesDebugViewModel> _logger;

    #region Observable Properties

    // MesService (锐派接口) 参数
    [ObservableProperty]
    private string _agvDeviceCode = "AGV001";

    [ObservableProperty]
    private string _palletId = "P00001";

    [ObservableProperty]
    private string _deviceCode = "Palletizing";

    [ObservableProperty]
    private int _jobId = 10000;

    [ObservableProperty]
    private int _palletType = 1;

    [ObservableProperty]
    private string _bagNum1 = "A001";

    [ObservableProperty]
    private decimal _quantity1 = 20;

    [ObservableProperty]
    private string _bagNum2 = "A002";

    [ObservableProperty]
    private decimal _quantity2 = 30;

    // 锐派接口密钥参数
    [ObservableProperty]
    private string _corpNo = "020";

    [ObservableProperty]
    private string _corpId = "IezQB0Esc1mN4Tf7Xw83U3tv7eEy33PJ";

    [ObservableProperty]
    private string _revopacAuthKey = string.Empty;

    // MesWebApi (工单接口) 参数
    [ObservableProperty]
    private string _workOrderCode = "WO20240101001";

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Today;

    [ObservableProperty]
    private string _lineNo = "LINE001";

    [ObservableProperty]
    private string _productCode = "P001";

    [ObservableProperty]
    private string _productName = "产品A";

    [ObservableProperty]
    private string _productSpec = "规格型号A";

    [ObservableProperty]
    private decimal _workOrderQuantity = 1000;

    [ObservableProperty]
    private string _unit = "kg";

    [ObservableProperty]
    private string _batchNumber = "BATCH001";

    [ObservableProperty]
    private string _labelTemplateCode = "LABEL001";

    [ObservableProperty]
    private int _status = 1;

    // Basic 认证参数
    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "123456";

    // 日志
    [ObservableProperty]
    private ObservableCollection<string> _logs = new();

    [ObservableProperty]
    private string _statusMessage = "就绪";

    #endregion

    #region Constructor

    public MesDebugViewModel(
        IMesService mesService,
        IMesWebApi mesWebApi,
        ILogger<MesDebugViewModel> logger)
    {
        _mesService = mesService;
        _mesWebApi = mesWebApi;
        _logger = logger;

        AddLog("MES 接口调试工具已启动");
        _logger.LogInformation("MES 接口调试工具已启动");
    }

    #endregion

    #region Command Methods

    [RelayCommand]
    private void GenerateRevopacAuthKey()
    {
        try
        {
            AddLog("========== 生成锐派密钥 ==========");
            AddLog($"CorpNo: {CorpNo}");
            AddLog($"CorpId: {CorpId}");

            // 获取当前时间戳（10位数字，精确到秒）
            var authSysTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            AddLog($"时间戳: {authSysTime}");

            // 生成签名字符串：auth_sys_time&Corpid
            var signString = $"{authSysTime}&{CorpId}";
            AddLog($"签名原文: {signString}");

            // MD5 加密
            var authSignCode = ComputeMd5Hash(signString);
            AddLog($"MD5签名: {authSignCode}");

            // 组合最终密钥：CorpNo&auth_sys_time&auth_sign_code
            RevopacAuthKey = $"{CorpNo}&{authSysTime}&{authSignCode}";
            AddLog($"? 生成的密钥: {RevopacAuthKey}");
            AddLog($"? 密钥有效期: 2分钟");

            StatusMessage = "? 锐派密钥已生成";
            _logger.LogInformation("锐派密钥已生成，CorpNo: {CorpNo}, 时间戳: {Timestamp}", CorpNo, authSysTime);
        }
        catch (Exception ex)
        {
            StatusMessage = $"? 生成密钥失败: {ex.Message}";
            AddLog($"? 异常：{ex.Message}");
            _logger.LogError(ex, "生成锐派密钥异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task FinishPalletizingAsync()
    {
        try
        {
            StatusMessage = "正在调用锐派码垛完成接口...";
            AddLog("========== 锐派码垛完成 ==========");
            AddLog($"认证密钥: {RevopacAuthKey}");
            AddLog($"AGV设备: {AgvDeviceCode}");
            AddLog($"托盘ID: {PalletId}");
            AddLog($"设备编号: {DeviceCode}");
            AddLog($"任务ID: {JobId}");
            AddLog($"打包明细: [{BagNum1}:{Quantity1}, {BagNum2}:{Quantity2}]");

            _logger.LogInformation("开始调用锐派码垛完成接口");

            var request = new FinishPalletizingRequest
            {
                AgvDeviceCode = AgvDeviceCode,
                PalletId = PalletId,
                DeviceCode = DeviceCode,
                JobId = JobId,
                List = new List<PackageDetail>
                {
                    new() { BagNums = BagNum1, Quan = Quantity1 },
                    new() { BagNums = BagNum2, Quan = Quantity2 }
                }
            };

            var response = await _mesService.FinishPalletizingAsync(request);

            if (response.IsSuccess)
            {
                StatusMessage = "? 锐派码垛完成接口调用成功";
                AddLog($"? 成功：{response.ErrorMsg}");
                _logger.LogInformation("锐派码垛完成接口调用成功");
            }
            else
            {
                StatusMessage = $"? 锐派码垛完成接口调用失败: {response.ErrorMsg}";
                AddLog($"? 失败：[{response.ErrorCode}] {response.ErrorMsg}");
                _logger.LogWarning("锐派码垛完成接口调用失败: {ErrorMsg}", response.ErrorMsg);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? 异常: {ex.Message}";
            AddLog($"? 异常：{ex.Message}");
            _logger.LogError(ex, "锐派码垛完成接口调用异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task ReportLackPalletAsync()
    {
        try
        {
            StatusMessage = "正在调用锐派托盘缺少接口...";
            AddLog("========== 锐派托盘缺少 ==========");
            AddLog($"认证密钥: {RevopacAuthKey}");
            AddLog($"AGV设备: {AgvDeviceCode}");
            AddLog($"托盘类型: {PalletType} ({(PalletType == 1 ? "母托盘" : "子托盘")})");

            _logger.LogInformation("开始调用锐派托盘缺少接口");

            var request = new LackPalletRequest
            {
                AgvDeviceCode = AgvDeviceCode,
                PalletType = PalletType
            };

            var response = await _mesService.ReportLackPalletAsync(request);

            if (response.IsSuccess)
            {
                StatusMessage = "? 锐派托盘缺少接口调用成功";
                AddLog($"? 成功：{response.ErrorMsg}");
                _logger.LogInformation("锐派托盘缺少接口调用成功");
            }
            else
            {
                StatusMessage = $"? 锐派托盘缺少接口调用失败: {response.ErrorMsg}";
                AddLog($"? 失败：[{response.ErrorCode}] {response.ErrorMsg}");
                _logger.LogWarning("锐派托盘缺少接口调用失败: {ErrorMsg}", response.ErrorMsg);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? 异常: {ex.Message}";
            AddLog($"? 异常：{ex.Message}");
            _logger.LogError(ex, "锐派托盘缺少接口调用异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task CreateWorkOrderAsync()
    {
        try
        {
            StatusMessage = "正在调用工单推送接口...";
            AddLog("========== MES 工单推送 ==========");
            AddLog($"工单号: {WorkOrderCode}");
            AddLog($"工单日期: {OrderDate:yyyy-MM-dd}");
            AddLog($"产线编号: {LineNo}");
            AddLog($"产品: {ProductCode} - {ProductName}");
            AddLog($"规格: {ProductSpec}");
            AddLog($"数量: {WorkOrderQuantity} {Unit}");
            AddLog($"批号: {BatchNumber}");
            AddLog($"状态: {Status} ({(Status == 1 ? "开工" : "完工")})");

            _logger.LogInformation("开始调用工单推送接口");

            var request = new WorkOrderRequest
            {
                Code = WorkOrderCode,
                OrderDate = OrderDate,
                LineNo = LineNo,
                ProductCode = ProductCode,
                ProductName = ProductName,
                ProductSpec = ProductSpec,
                Quantity = WorkOrderQuantity,
                Unit = Unit,
                BatchNumber = BatchNumber,
                LabelTemplateCode = LabelTemplateCode,
                Status = Status,
                OrderData = new List<OrderDataItem>
                {
                    new() { Key = "key1", Name = "名称1", Value = "值1" },
                    new() { Key = "key2", Name = "名称2", Value = "值2" }
                }
            };

            var response = await _mesWebApi.CreateWorkOrderAsync(request);

            if (response.IsSuccess)
            {
                StatusMessage = "? 工单推送接口调用成功";
                AddLog($"? 成功：{response.ErrorMsg}");
                _logger.LogInformation("工单推送接口调用成功");
            }
            else
            {
                StatusMessage = $"? 工单推送接口调用失败: {response.ErrorMsg}";
                AddLog($"? 失败：[{response.ErrorCode}] {response.ErrorMsg}");
                _logger.LogWarning("工单推送接口调用失败: {ErrorMsg}", response.ErrorMsg);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? 异常: {ex.Message}";
            AddLog($"? 异常：{ex.Message}");
            _logger.LogError(ex, "工单推送接口调用异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private void SetBasicAuth()
    {
        try
        {
            AddLog("========== 设置 Basic 认证 ==========");
            AddLog($"用户名: {Username}");
            AddLog($"密码: {new string('*', Password.Length)}");

            _mesWebApi.SetBasicAuth(Username, Password);

            StatusMessage = "? Basic 认证已设置";
            AddLog("? Basic 认证已设置");
            _logger.LogInformation("Basic 认证已设置，用户名: {Username}", Username);
        }
        catch (Exception ex)
        {
            StatusMessage = $"? 设置认证失败: {ex.Message}";
            AddLog($"? 异常：{ex.Message}");
            _logger.LogError(ex, "设置 Basic 认证异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        StatusMessage = "日志已清空";
        _logger.LogInformation("日志已清空");
    }

    #endregion

    #region Helper Methods

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Logs.Add($"[{timestamp}] {message}");
    }

    /// <summary>
    /// 计算 MD5 哈希值
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>MD5 哈希值（32位小写）</returns>
    private static string ComputeMd5Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    #endregion
}
