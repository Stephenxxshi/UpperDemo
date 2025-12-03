using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using Plant01.Upper.Application.Models.Logging;
using Plant01.Upper.Presentation.Core.ViewModels;
using Serilog;

namespace Plant01.Upper.Presentation.Bootstrapper;

public static class Bootstrapper
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        // Build intermediate config to read LoggingProvider
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var loggingProvider = config["LoggingProvider"];
        var builder = Host.CreateDefaultBuilder(args);

        // Configure Serilog if selected
        if (string.Equals(loggingProvider, "Serilog", StringComparison.OrdinalIgnoreCase))
        {
            builder.UseSerilog((context, services, configuration) =>
            {
                var logStore = services.GetRequiredService<ILogStore>();
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Sink(new SerilogToLogStoreSink(logStore));
            });
        }
        // Configure NLog if selected
        else if (string.Equals(loggingProvider, "NLog", StringComparison.OrdinalIgnoreCase))
        {
            builder.UseNLog();
            builder.ConfigureServices(services =>
            {
                services.AddHostedService<NLogConfigurationService>();
            });
        }

        builder.ConfigureServices((context, services) =>
        {
            ConfigureCommonServices(services, loggingProvider);
        });

        return builder;
    }

    private static void ConfigureCommonServices(IServiceCollection services, string? loggingProvider)
    {
        // Logging Store (Shared by all providers)
        services.AddSingleton<ILogStore, LogStore>();

        // Register Default Logger only if no other provider is selected
        if (string.IsNullOrEmpty(loggingProvider) || string.Equals(loggingProvider, "Default", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<ILoggerProvider, ObservableLoggerProvider>();
        }

        // 注册通用的 ViewModel
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ProduceRecordViewModel>();

        // 这里可以继续注册其他通用服务，例如 HTTP Client, AutoMapper 等
    }
}
