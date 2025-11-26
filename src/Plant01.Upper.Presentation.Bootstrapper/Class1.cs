using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Plant01.Upper.Presentation.Bootstrapper;

public static class ClientServiceExtensions
{
    // 这个方法包含了所有通用的业务注册逻辑
    public static IServiceCollection AddIndustrialAutomationClient(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. 基础设施
        //services.AddOpcUaHardware(configuration);

        // 2. 核心 Shell
        //services.AddSingleton<ShellViewModel>();

        // 3. 业务模块 (以后加新模块只改这里)
        //services.AddTransient<Presentation.Core.Features.Monitoring.MonitorViewModel>();
        //services.AddTransient<Presentation.Core.Features.Settings.SettingsViewModel>();

        return services;
    }
}
