using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
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
    private readonly SynchronizationContext? _uiContext;

    #region Observable Properties

    // MesService (生成接口) 参数
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

    // 生成接口密钥参数
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

    [ObservableProperty]
    private bool _isServerRunning;

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
        _uiContext = SynchronizationContext.Current;

        _mesWebApi.OnWorkOrderReceived += OnWorkOrderReceivedHandler;

        AddLog("MES 接口调试工具初始化完成");
        _logger.LogInformation("MES 接口调试工具初始化完成");
    }

    #endregion

    #region Event Handlers

    private Task<WorkOrderResponse> OnWorkOrderReceivedHandler(WorkOrderRequest request)
    {
        RunOnUiThread(() =>
        {
            AddLog($"收到工单推送: {request.Code}");
            AddLog($"  产品: {request.ProductName} ({request.ProductCode})");
            AddLog($"  数量: {request.Quantity} {request.Unit}");
            AddLog($"  状态: {request.Status}");
            _logger.LogInformation("收到工单推送: {Code}", request.Code);
        });

        return Task.FromResult(new WorkOrderResponse { ErrorCode = 0, ErrorMsg = "接收成功" });
    }

    #endregion

    #region Command Methods

    [RelayCommand]
    private async Task StartServerAsync()
    {
        try
        {
            StatusMessage = "正在启动 Web API 服务...";
            await _mesWebApi.StartAsync();
            IsServerRunning = true;
            StatusMessage = "Web API 服务已启动";
            AddLog("Web API 服务已启动");
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动失败: {ex.Message}";
            AddLog($"启动失败: {ex.Message}");
            _logger.LogError(ex, "启动 Web API 服务失败");
        }
    }

    [RelayCommand]
    private async Task StopServerAsync()
    {
        try
        {
            StatusMessage = "正在停止 Web API 服务...";
            await _mesWebApi.StopAsync();
            IsServerRunning = false;
            StatusMessage = "Web API 服务已停止";
            AddLog("Web API 服务已停止");
        }
        catch (Exception ex)
        {
            StatusMessage = $"停止失败: {ex.Message}";
            AddLog($"停止失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GenerateRevopacAuthKey()
    {
        try
        {
            AddLog("========== 生成认证密钥 ==========");
            AddLog($"CorpNo: {CorpNo}");
            AddLog($"CorpId: {CorpId}");

            // 获取当前时间戳（10位数字，精确到秒）
            var authSysTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            AddLog($"时间戳: {authSysTime}");

            // 构造签名字符串：auth_sys_time&Corpid
            var signString = $"{authSysTime}&{CorpId}";
            AddLog($"签名原串: {signString}");

            // MD5 加密
            var authSignCode = ComputeMd5Hash(signString);
            AddLog($"MD5签名: {authSignCode}");

            // 生成最终密钥：CorpNo&auth_sys_time&auth_sign_code
            RevopacAuthKey = $"{CorpNo}&{authSysTime}&{authSignCode}";
            AddLog($"✅ 生成的密钥: {RevopacAuthKey}");
            AddLog($"ℹ️ 密钥有效期: 2分钟");

            StatusMessage = "✅ 密钥生成成功";
            _logger.LogInformation("生成密钥成功，CorpNo: {CorpNo}, 时间戳: {Timestamp}", CorpNo, authSysTime);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 生成密钥失败: {ex.Message}";
            AddLog($"❌ 异常：{ex.Message}");
            _logger.LogError(ex, "生成认证密钥异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task FinishPalletizingAsync()
    {
        try
        {
            StatusMessage = "正在调用完工回传接口...";
            AddLog("========== 完工回传 ==========");
            AddLog($"认证密钥: {RevopacAuthKey}");
            AddLog($"AGV设备: {AgvDeviceCode}");
            AddLog($"托盘ID: {PalletId}");
            AddLog($"设备码: {DeviceCode}");
            AddLog($"任务ID: {JobId}");
            AddLog($"包装明细: [{BagNum1}:{Quantity1}, {BagNum2}:{Quantity2}]");

            _logger.LogInformation("开始调用完工回传接口");

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
                StatusMessage = "✅ 完工回传接口调用成功";
                AddLog($"✅ 成功：{response.ErrorMsg}");
                _logger.LogInformation("完工回传接口调用成功");
            }
            else
            {
                StatusMessage = $"❌ 完工回传接口调用失败: {response.ErrorMsg}";
                AddLog($"❌ 失败：[{response.ErrorCode}] {response.ErrorMsg}");
                _logger.LogWarning("完工回传接口调用失败: {ErrorMsg}", response.ErrorMsg);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 异常: {ex.Message}";
            AddLog($"❌ 异常：{ex.Message}");
            _logger.LogError(ex, "完工回传接口调用异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task ReportLackPalletAsync()
    {
        try
        {
            StatusMessage = "正在调用缺托盘接口...";
            AddLog("========== 缺托盘 ==========");
            AddLog($"认证密钥: {RevopacAuthKey}");
            AddLog($"AGV设备: {AgvDeviceCode}");
            AddLog($"托盘类型: {PalletType} ({(PalletType == 1 ? "母托盘" : "子托盘")})");

            _logger.LogInformation("开始调用缺托盘接口");

            var request = new LackPalletRequest
            {
                AgvDeviceCode = AgvDeviceCode,
                PalletType = PalletType
            };

            var response = await _mesService.ReportLackPalletAsync(request);

            if (response.IsSuccess)
            {
                StatusMessage = "✅ 缺托盘接口调用成功";
                AddLog($"✅ 成功：{response.ErrorMsg}");
                _logger.LogInformation("缺托盘接口调用成功");
            }
            else
            {
                StatusMessage = $"❌ 缺托盘接口调用失败: {response.ErrorMsg}";
                AddLog($"❌ 失败：[{response.ErrorCode}] {response.ErrorMsg}");
                _logger.LogWarning("缺托盘接口调用失败: {ErrorMsg}", response.ErrorMsg);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 异常: {ex.Message}";
            AddLog($"❌ 异常：{ex.Message}");
            _logger.LogError(ex, "缺托盘接口调用异常");
        }

        AddLog("=====================================");
        AddLog("");
    }

    [RelayCommand]
    private async Task SimulatePushAsync()
    {
        try
        {
            StatusMessage = "正在模拟工单推送...";
            AddLog("========== 模拟工单推送 (Client -> Localhost) ==========");
            AddLog($"工单号: {WorkOrderCode}");
            AddLog($"目标地址: http://localhost:5000/api/work_order/create");

            using var client = new HttpClient();
            
            // 添加 Basic 认证
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }

            var request = new
            {
                code = WorkOrderCode,
                orderDate = OrderDate.ToString("yyyy-MM-dd"),
                lineNo = LineNo,
                productCode = ProductCode,
                productName = ProductName,
                productSpec = ProductSpec,
                quantity = WorkOrderQuantity,
                unit = Unit,
                batchNumber = BatchNumber,
                labelTemplateCode = LabelTemplateCode,
                status = Status,
                orderData = new[]
                {
                    new { key = "key1", name = "属性1", value = "值1" },
                    new { key = "key2", name = "属性2", value = "值2" }
                }
            };

            var response = await client.PostAsJsonAsync("http://localhost:5000/api/work_order/create", request);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "✅ 模拟推送发送成功";
                AddLog($"✅ 发送成功: {response.StatusCode}");
                AddLog($"   响应: {result}");
            }
            else
            {
                StatusMessage = $"❌ 模拟推送发送失败: {response.StatusCode}";
                AddLog($"❌ 发送失败: {response.StatusCode}");
                AddLog($"   响应: {result}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 异常: {ex.Message}";
            AddLog($"❌ 异常：{ex.Message}");
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

    private void RunOnUiThread(Action action)
    {
        if (_uiContext != null)
        {
            _uiContext.Post(_ => action(), null);
        }
        else
        {
            action();
        }
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
