using Microsoft.EntityFrameworkCore; // Add EF Core namespace
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Hosting;

using Plant01.Infrastructure.Shared.Extensions;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Mappings; // 确保引用了 Mapping Profile 所在的命名空间
using Plant01.Upper.Application.Models.Logging;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Infrastructure.Repository;
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

        // 注册 HttpService（使用现代化的 IHttpClientFactory）
        services.AddHttpService(builder =>
        {
            // 可选：配置 Polly 重试策略（需要安装 Microsoft.Extensions.Http.Resilience）
            // builder.AddStandardResilienceHandler();
        });

        // 注册应用服务
        services.AddSingleton<IMesWebApi, MesWebApi>();
        services.AddScoped<IMesService, MesService>();
        services.AddSingleton<IMesCommandService, MesCommandService>(); // 改为 Singleton
        services.AddScoped<IPlcFlowService, PlcFlowService>();       // 新增
        services.AddScoped<IProductionQueryService, ProductionQueryService>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        // 注册 AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<ProductionMappingProfile>());

        // 注册 DbContext (PostgreSQL)
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        // 添加 DbContextFactory 注册
        services.AddDbContextFactory<AppDbContext>((serviceProvider,options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });


        // 确保 UnitOfWork 已注册
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        // 注册通用的 ViewModel
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ProduceRecordViewModel>();
        services.AddSingleton<ProductionMonitorViewModel>();
        services.AddSingleton<WorkOrderListViewModel>();

        // 注册 MES 调试 ViewModel
        services.AddSingleton<MesDebugViewModel>();

        // 这里可以继续注册其他通用服务，例如 AutoMapper 等
    }
}
