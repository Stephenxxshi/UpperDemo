using System.ComponentModel.DataAnnotations;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

/// <summary>
/// DeviceConfig 扩展方法
/// </summary>
public static class DeviceConfigExtensions
{
    /// <summary>
    /// 获取并验证驱动配置
    /// </summary>
    public static T GetAndValidateDriverConfig<T>(this DeviceConfig config) where T : class, new()
    {
        var driverConfig = config.GetDriverConfig<T>();
        
        if (driverConfig == null)
            throw new ArgumentException("无法获取驱动配置");
        
        var validationContext = new ValidationContext(driverConfig);
        var validationResults = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(driverConfig, validationContext, validationResults, true))
        {
            var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            throw new ArgumentException($"驱动配置验证失败: {errors}");
        }
        
        return driverConfig;
    }
}
