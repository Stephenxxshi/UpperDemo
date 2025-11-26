using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plant01.Upper.Presentation.Core.ViewModels;

namespace Plant01.Upper.Presentation.Bootstrapper
{
    public static class Bootstrapper
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    ConfigureCommonServices(services);
                });
        }

        private static void ConfigureCommonServices(IServiceCollection services)
        {
            // 注册通用的 ViewModel
            services.AddSingleton<ShellViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<SettingsViewModel>();
            
            // 这里可以继续注册其他通用服务，例如 HTTP Client, AutoMapper 等
        }
    }
}
