using Microsoft.EntityFrameworkCore; // Add EF Core namespace
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Hosting;

using Plant01.Domain.Shared.Events;
using Plant01.Infrastructure.Shared.Extensions;
using Plant01.Upper.Application.EventHandlers;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Mappings; // 确保引用了 Mapping Profile 所在的命名空间
using Plant01.Upper.Application.Models.Logging;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Application.Workstations;
using Plant01.Upper.Application.Workstations.Processors;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Infrastructure.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Configs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;
using Plant01.Upper.Infrastructure.DeviceCommunication.Engine;
using Plant01.Upper.Infrastructure.Repository;
using Plant01.Upper.Infrastructure.Services;
using Plant01.Upper.Presentation.Core.ViewModels;

using Serilog;

namespace Plant01.Upper.Presentation.Bootstrapper;

public static class Bootstrapper
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var upperEnvironment = Environment.GetEnvironmentVariable("Upper");
        // 构建中间配置以读取 LoggingProvider
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{upperEnvironment}.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
        {
            if (!string.IsNullOrEmpty(upperEnvironment))
            {
                configBuilder.AddJsonFile($"appsettings.{upperEnvironment}.json", optional: true, reloadOnChange: true);
            }
        });
        var loggingProvider = config["LoggingProvider"];

        // 如果选择了 Serilog，则进行配置
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
        // 如果选择了 NLog，则进行配置
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
        // 日志存储 (所有提供程序共享)
        services.AddSingleton<ILogStore, LogStore>();

        // 仅当未选择其他提供程序时才注册默认记录器
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
        services.AddSingleton<WorkOrderPushCommandHandle>();
        services.AddSingleton<IWorkOrderPushCommandHandle>(sp => sp.GetRequiredService<WorkOrderPushCommandHandle>());

        // 注册通用触发与监控服务
        services.AddSingleton<TriggerDispatcherService>();
        services.AddHostedService<TriggerDispatcherService>(sp => sp.GetRequiredService<TriggerDispatcherService>());
        services.AddSingleton<ITriggerDispatcher>(sp => sp.GetRequiredService<TriggerDispatcherService>());

        // 注册设备监控服务 (原 PlcMonitorService)
        services.AddHostedService<DeviceMonitorService>();

        // 注册 MVVM Messenger (修复 DeviceMonitorService 的依赖报错)
        services.AddSingleton<CommunityToolkit.Mvvm.Messaging.IMessenger>(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default);


        // 注册驱动
        services.AddTransient<DriverFactory>();
        services.AddTransient<SiemensS7Driver>();
        services.AddTransient<ModbusTcpDriver>();
        services.AddTransient<SimulationDriver>();
        services.AddTransient<WsdomInkjetTcpDriver>();
        services.AddTransient<Func<string, IDriver>>(sp => name => sp.GetRequiredService<DriverFactory>().CreateDriver(name));

        // 注册领域事件总线
        services.AddSingleton<IDomainEventBus, DomainEventBus>();

        // 注册事件处理器
        services.AddScoped<MesEventHandler>();
        services.AddHostedService<EventRegistrationService>(); // 注册事件绑定服务

        services.AddScoped<IProductionQueryService, ProductionQueryService>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

        // 注册产线配置管理器(内存式管理)
        services.AddSingleton<ProductionConfigManager>();

        // 注册设备配置服务(从独立配置文件加载)
        services.AddSingleton<EquipmentConfigService>();
        services.AddSingleton<IEquipmentConfigService>(sp => sp.GetRequiredService<EquipmentConfigService>());

        // 注册产线配置初始化服务(依赖 EquipmentConfigService)
        services.AddHostedService<ProductionLineConfigService>();

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
        services.AddDbContextFactory<AppDbContext>((serviceProvider, options) =>
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
        services.AddSingleton<PlcDebugViewModel>();
        services.AddSingleton<MesInterfaceDebugViewModel>();
        services.AddSingleton<MesDebugViewModel>();

        // 注册设备通信层 (Device Communication Layer)
        // 1. 注册配置加载器
        services.AddSingleton<MultiFormatConfigLoader>();
        services.AddSingleton<ConfigurationLoader>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConfigurationLoader>>();
            var multiFormatLoader = sp.GetRequiredService<MultiFormatConfigLoader>();
            // 假设 Configs 文件夹在运行目录下
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            return new ConfigurationLoader(configPath, logger, multiFormatLoader);
        });

        // 2. 注册热重载服务
        services.AddSingleton<ConfigHotReloader>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConfigHotReloader>>();
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            return new ConfigHotReloader(configPath, logger);
        });

        // 3. 注册驱动工厂和引擎
        services.AddSingleton<DriverFactory>();
        services.AddSingleton<TagEngine>();

        // 注册标签生成服务
        services.AddSingleton<ITagGenerationService, TagGenerationService>();

        // 4. 注册主服务 (既是业务接口，又是后台服务)
        services.AddSingleton<DeviceCommunicationService>();
        services.AddSingleton<IDeviceCommunicationService>(sp => sp.GetRequiredService<DeviceCommunicationService>());
        services.AddHostedService(sp => sp.GetRequiredService<DeviceCommunicationService>());

        // 1. 注册工位处理器
        services.AddSingleton<IWorkstationProcessor, PackagingWorkstationProcessor>();
        services.AddSingleton<IWorkstationProcessor, CheckweigherWorkStationProcessor>();
        services.AddSingleton<IWorkstationProcessor, InkjetWorkStationProcessor>();
        services.AddSingleton<IWorkstationProcessor, PalletizerWorkStationProcessor>();
        services.AddSingleton<IWorkstationProcessor, PalletOutWorkStationProcessor>();
        services.AddSingleton<IWorkstationProcessor, StretchWrapperWorkStationProcessor>();
        services.AddSingleton<IWorkstationProcessor, LabelingWorkStationProcessor>();

        // 2. 注册工位流程服务（监听触发标签）
        services.AddHostedService<WorkstationProcessService>();

        // 3. 注册心跳服务（向PLC发送心跳）
        services.AddHostedService<HeartbeatService>();

        // 这里可以继续注册其他通用服务，例如 AutoMapper 等
    }
}
