using Plant01.Domain.Shared.Interfaces;
using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.Api.Responses;
using Plant01.Upper.Application.Interfaces;

using System.Security.Cryptography;
using System.Text;

namespace Plant01.Upper.Application.Services;

/// <summary>
/// MES 服务实现
/// </summary>
public class MesService : IMesService
{
    private readonly IHttpService _httpService;
    private readonly ILogger<MesService> _logger;
    private readonly string _baseUrl;
    private readonly string _corpNo;
    private readonly string _corpId;

    public MesService(
        IHttpService httpService,
        ILogger<MesService> logger,
        IConfiguration configuration)
    {
        _httpService = httpService;
        _logger = logger;
        _baseUrl = configuration["MesApi:BaseUrl"] ?? throw new InvalidOperationException("MesApi:BaseUrl 配置不能为空");
        _corpNo = configuration["MesApi:CorpNo"] ?? throw new InvalidOperationException("MesApi:CorpNo 配置不能为空");
        _corpId = configuration["MesApi:CorpId"] ?? throw new InvalidOperationException("MesApi:CorpId 配置不能为空");
    }

    /// <inheritdoc/>
    public async Task<MesApiResponse> FinishPalletizingAsync(FinishPalletizingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            _logger.LogInformation("调用锐派码垛完成接口，AGV: {AgvDeviceCode}, 托盘ID: {PalletId}, No: {JobNo}",
                request.AgvDeviceCode, request.PalletId, request.JobNo);

            // 生成认证密钥
            var authKey = GenerateAuthKey();
            _httpService.AddHeader("Authorization", authKey);
            _httpService.AddHeader("Accept", "application/json");

            var url = $"{_baseUrl}/cmsDeviceData/REVOPACFinishPalletizing";

            // 构建请求体（使用小写驼峰命名）
            var requestBody = new
            {
                agvDeviceCode = request.AgvDeviceCode,
                palletId = request.PalletId,
                deviceCode = request.DeviceCode,
                jobNo = request.JobNo,
                list = request.List.Select(item => new
                {
                    bagNums = item.BagNums,
                    quan = item.Quan
                }).ToList()
            };

            var response = await _httpService.PostJsonAsync<object, MesApiResponse>(url, requestBody, cancellationToken);

            if (response?.IsSuccess == true)
            {
                _logger.LogInformation("锐派码垛完成接口调用成功，任务ID: {jobNo}", request.JobNo);
            }
            else
            {
                _logger.LogWarning("锐派码垛完成接口调用失败，任务ID: {jobNo}, 错误码: {ErrorCode}, 错误信息: {ErrorMsg}",
                    request.JobNo, response?.ErrorCode, response?.ErrorMsg);
            }

            return response ?? new MesApiResponse { ErrorCode = -1, ErrorMsg = "响应为空" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调用锐派码垛完成接口异常，任务ID: {jobNo}", request.JobNo);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MesApiResponse> ReportLackPalletAsync(LackPalletRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            _logger.LogInformation("调用锐派托盘缺少接口，AGV: {AgvDeviceCode}, 托盘类型: {PalletType}",
                request.AgvDeviceCode, request.PalletType);

            // 生成认证密钥
            var authKey = GenerateAuthKey();
            _httpService.AddHeader("Authorization", authKey);
            _httpService.AddHeader("Accept", "application/json");

            var url = $"{_baseUrl}/cmsDeviceData/REVOPACLackPallet";

            // 构建请求体（使用小写驼峰命名）
            var requestBody = new
            {
                agvDeviceCode = request.AgvDeviceCode,
                palletType = request.PalletType
            };

            var response = await _httpService.PostJsonAsync<object, MesApiResponse>(url, requestBody, cancellationToken);

            if (response?.IsSuccess == true)
            {
                _logger.LogInformation("锐派托盘缺少接口调用成功，AGV: {AgvDeviceCode}", request.AgvDeviceCode);
            }
            else
            {
                _logger.LogWarning("锐派托盘缺少接口调用失败，AGV: {AgvDeviceCode}, 错误码: {ErrorCode}, 错误信息: {ErrorMsg}",
                    request.AgvDeviceCode, response?.ErrorCode, response?.ErrorMsg);
            }

            return response ?? new MesApiResponse { ErrorCode = -1, ErrorMsg = "响应为空" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调用锐派托盘缺少接口异常，AGV: {AgvDeviceCode}", request.AgvDeviceCode);
            throw;
        }
    }

    /// <summary>
    /// 生成认证密钥
    /// </summary>
    /// <remarks>
    /// 密钥格式：CorpNo&auth_sys_time&auth_sign_code
    /// - auth_sys_time：当前时间戳（10位数字，精确到秒）
    /// - auth_sign_code：MD5("auth_sys_time&CorpId")
    /// 注意：密钥有效期2分钟
    /// </remarks>
    /// <returns>认证密钥</returns>
    private string GenerateAuthKey()
    {
        // 获取当前时间戳（10位数字，精确到秒）
        var authSysTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 生成签名字符串：auth_sys_time&Corpid
        var signString = $"{authSysTime}&{_corpId}";

        // MD5 加密
        var authSignCode = ComputeMd5Hash(signString);

        // 组合最终密钥：CorpNo&auth_sys_time&auth_sign_code
        var authKey = $"{_corpNo}&{authSysTime}&{authSignCode}";

        _logger.LogDebug("生成认证密钥，时间戳: {AuthSysTime}, 签名: {AuthSignCode}", authSysTime, authSignCode);

        return authKey;
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
    /// 上报托盘完成
    /// </summary>
    /// <param name="workOrderCode">工单编码</param>
    /// <param name="palletCode">托盘编码</param>
    public async Task ReportPalletCompletionAsync(string workOrderCode, string palletCode)
    {
        _logger.LogInformation("Reporting pallet completion for WorkOrder: {WorkOrder}, Pallet: {Pallet}", workOrderCode, palletCode);

        // 构造请求对象 (实际项目中需要从数据库查询详细信息)
        var request = new FinishPalletizingRequest
        {
            AgvDeviceCode = "AGV_DEFAULT", // TODO: 配置或查找
            PalletId = palletCode,
            DeviceCode = "ROBOT_01",       // TODO: 配置
            JobNo = workOrderCode,
            List = new List<PackageDetail>() // TODO: 查询托盘内的包明细
        };

        try
        {
            await FinishPalletizingAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report pallet completion to MES");
            // 根据业务需求决定是否抛出异常
        }
    }
}
