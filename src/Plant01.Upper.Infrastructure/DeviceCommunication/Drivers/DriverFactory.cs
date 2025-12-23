using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Microsoft.Extensions.DependencyInjection;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class DriverFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DriverFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDriver CreateDriver(string driverName)
    {
        // 在实际场景中，可能会使用带键的服务或更复杂的查找机制
        // 目前为了简单起见，针对所有情况返回 SimulationDriver，或在实现时返回特定驱动
        
        if (string.Equals(driverName, "SiemensS7", StringComparison.OrdinalIgnoreCase))
        {
            return _serviceProvider.GetService<SiemensS7Driver>()!;
        }
        else if (string.Equals(driverName, "Modbus", StringComparison.OrdinalIgnoreCase))
        {
            // 当实现 Modbus 驱动时，这里应返回 Modbus 驱动
            return _serviceProvider.GetService<ModbusTcpDriver>()!;
        }
        else if(string.Equals(driverName, "WsdomInkjetTcpDriver", StringComparison.OrdinalIgnoreCase))
        {
            return _serviceProvider.GetService<WsdomInkjetTcpDriver>()!;
        }
        // 默认回退
        return _serviceProvider.GetService<SimulationDriver>()!;
    }
}
