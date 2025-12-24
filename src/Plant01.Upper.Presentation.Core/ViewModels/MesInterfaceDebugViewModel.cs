using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plant01.Domain.Shared.Interfaces;
using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.Api.Responses;
using Plant01.Upper.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Plant01.Upper.Presentation.Core.ViewModels;

/// <summary>
/// MES 接口调试 ViewModel
/// </summary>
public partial class MesInterfaceDebugViewModel : ObservableObject
{
    private readonly IMesService _mesService;
    private readonly IMesWebApi _mesWebApi;
    private readonly IHttpService _httpService;
    private readonly ILogger<MesInterfaceDebugViewModel> _logger;
    private readonly SynchronizationContext? _uiContext;

    #region 依赖属性

    // MesService (生成接口) 参数
    [ObservableProperty]
    private string _agvDeviceCode = "AGV1";

    [ObservableProperty]
    private string _palletId = "P00001";

    [ObservableProperty]
    private string _deviceCode = "MDJ1";

    [ObservableProperty]
    private string _jobId = "MO010604:1";

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
    private string _workOrderCode = "MO010604:1";

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Today;

    [ObservableProperty]
    private string _lineNo = "ZL004";

    [ObservableProperty]
    private string _productCode = "020101780";

    [ObservableProperty]
    private string _productName = "SM103";

    [ObservableProperty]
    private string _productSpec = "000001";

    [ObservableProperty]
    private decimal _workOrderQuantity = 1000;

    [ObservableProperty]
    private string _unit = "kg";

    [ObservableProperty]
    private string _batchNumber = "C253572A";

    [ObservableProperty]
    private string _labelTemplateCode = "LABEL001";

    [ObservableProperty]
    private int _status = 1;

    // Basic 认证参数
    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "123456";

    [ObservableProperty]
    //private string _baseUrl = "http://localhost:5000";
    private string _baseUrl = "http://2bm09ua35806.vicp.fun:41916";

    // 日志
    [ObservableProperty]
    private ObservableCollection<string> _logs = new();

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private bool _isServerRunning;

    #endregion

    public MesInterfaceDebugViewModel(
        IMesService mesService,
        IMesWebApi mesWebApi,
        IHttpService httpService,
        ILogger<MesInterfaceDebugViewModel> logger)
    {
        _mesService = mesService;
        _mesWebApi = mesWebApi;
        _httpService = httpService;
        _logger = logger;
        _uiContext = SynchronizationContext.Current;
    }

    #region 事件处理器

    public Task<WorkOrderResponseDto> OnWorkOrderReceivedHandler(WorkOrderRequestDto request)
    {
        RunOnUiThread(() =>
        {
            AddLog($"收到工单推送: {request.Code}");
            AddLog($"  产品: {request.ProductName} ({request.ProductCode})");
            AddLog($"  数量: {request.Quantity} {request.Unit}");
            AddLog($"  状态: {request.Status}");
            _logger.LogInformation("收到工单推送: {Code}", request.Code);
        });

        return Task.FromResult(new WorkOrderResponseDto { ErrorCode = 0, ErrorMsg = "接收成功" });
    }

    #endregion

    #region 命令方法

    [RelayCommand]
    private async Task StartServerAsync()
    {
        try
        {
            StatusMessage = "正在启动 Web API 服务...";
            AddLog("========== 启动 Web API 服务 ==========");

            // 检查端口是否已被占用
            if (await IsPortInUseAsync(5000))
            {
                AddLog("⚠️ 警告：端口 5000 已被占用");
                AddLog("   尝试查找占用进程：使用命令 netstat -ano | findstr :5000");
            }

            await _mesWebApi.StartAsync();

            // 等待服务完全启动
            await Task.Delay(1000);

            // 验证服务是否真正可用
            bool isActuallyRunning = await VerifyServerHealthAsync();

            if (isActuallyRunning)
            {
                IsServerRunning = true;
                StatusMessage = "✅ Web API 服务已启动并验证成功";
                AddLog($"✅ Web API 服务已启动");
                AddLog($"✅ 服务健康检查通过");
                AddLog($"   监听地址: {BaseUrl}");
                AddLog($"   已注册路由: POST /api/work_order/create");
                AddLog($"   认证方式: Basic Auth ({Username})");
            }
            else
            {
                IsServerRunning = false;
                StatusMessage = "⚠️ 服务启动但无法访问";
                AddLog("⚠️ 警告：服务已启动但健康检查失败");
                AddLog("   可能原因：");
                AddLog("   1. 端口被其他进程占用");
                AddLog("   2. 防火墙或代理拦截");
                AddLog("   3. 之前的调试会话未完全停止");
                AddLog("   建议：重启 Visual Studio 或终止占用端口的进程");
            }

            AddLog("=====================================");
            AddLog("");
        }
        catch (Exception ex)
        {
            IsServerRunning = false;
            StatusMessage = $"❌ 启动失败: {ex.Message}";
            AddLog($"❌ 启动失败: {ex.Message}");
            AddLog($"   异常类型: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                AddLog($"   内部异常: {ex.InnerException.Message}");
            }
            _logger.LogError(ex, "启动 Web API 服务失败");
            AddLog("=====================================");
            AddLog("");
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
                JobNo = JobId,
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
            AddLog($"目标地址: {BaseUrl}/api/work_order/create");

            // 设置 Basic 认证
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
                _httpService.AddHeader("Authorization", $"Basic {authValue}");
                AddLog($"🔐 已添加 Basic 认证: {Username}:***");
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

            AddLog($"📤 发送请求...");

            var response = await _httpService.PostJsonAsync<object, WorkOrderResponseDto>(
                $"{BaseUrl}/api/work_order/create",
                request);

            if (response != null && response.ErrorCode == 0)
            {
                StatusMessage = "✅ 模拟推送发送成功";
                AddLog($"✅ 发送成功");
                AddLog($"   响应: {response.ErrorMsg}");
            }
            else
            {
                StatusMessage = $"❌ 模拟推送发送失败";
                AddLog($"❌ 发送失败");
                AddLog($"   响应内容: {response?.ErrorMsg ?? "(空)"}");
            }
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = $"❌ 网络异常: {ex.Message}";
            AddLog($"❌ 网络异常：{ex.Message}");
            if (ex.InnerException != null)
            {
                AddLog($"   根本原因: {ex.InnerException.Message}");
            }

            // 诊断常见问题
            if (ex.Message.Contains("502") || ex.Message.Contains("Bad Gateway"))
            {
                AddLog($"");
                AddLog($"💡 502 Bad Gateway 诊断：");
                AddLog($"   ❌ 请求未到达您的服务器");
                AddLog($"   可能原因：");
                AddLog($"   1. 端口 5000 被另一个进程占用");
                AddLog($"   2. 系统代理或防病毒软件拦截");
                AddLog($"   3. 旧的调试会话进程仍在运行");
            }
            else if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
            {
                AddLog($"");
                AddLog($"💡 401 Unauthorized 诊断：");
                AddLog($"   认证失败，请检查用户名和密码是否正确");
            }
        }
        catch (TaskCanceledException)
        {
            StatusMessage = $"❌ 请求超时";
            AddLog($"❌ 请求超时：服务可能未响应或处理时间过长");
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ 异常: {ex.Message}";
            AddLog($"❌ 异常：{ex.Message}");
            AddLog($"   类型: {ex.GetType().Name}");
        }
        finally
        {
            // 清除认证头，避免影响其他请求
            _httpService.RemoveHeader("Authorization");
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

    #region 辅助方法

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

    /// <summary>
    /// 验证服务器健康状态
    /// </summary>
    private async Task<bool> VerifyServerHealthAsync()
    {
        try
        {
            await _httpService.GetAsync(BaseUrl);
            AddLog($"🔍 健康检查: 服务器响应正常");
            return true;
        }
        catch (HttpRequestException ex)
        {
            AddLog($"🔍 健康检查失败: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException)
        {
            AddLog($"🔍 健康检查超时");
            return false;
        }
        catch (Exception ex)
        {
            AddLog($"🔍 健康检查异常: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> IsPortInUseAsync(int port)
    {
        try
        {
            await _httpService.GetAsync($"http://localhost:{port}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
