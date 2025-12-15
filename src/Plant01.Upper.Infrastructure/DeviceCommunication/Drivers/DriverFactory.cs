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
        // In a real scenario, you might use a keyed service or a more sophisticated lookup
        // For now, we just return SimulationDriver for everything or specific ones if implemented
        
        if (string.Equals(driverName, "SiemensS7", StringComparison.OrdinalIgnoreCase))
        {
            return new SiemensS7Driver(); 
        }
        else if (string.Equals(driverName, "Modbus", StringComparison.OrdinalIgnoreCase))
        {
            // Return Modbus driver when implemented
            return new SimulationDriver();
        }
        
        // Default fallback
        return new SimulationDriver();
    }
}
