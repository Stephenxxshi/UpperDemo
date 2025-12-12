using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class SimulationDriver : IDriver
{
    private bool _isConnected;
    private readonly Random _random = new();

    public bool IsConnected => _isConnected;

    public Task ConnectAsync()
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<Tag> tags)
    {
        var result = new Dictionary<string, object?>();
        
        if (!_isConnected) return Task.FromResult(result);

        foreach (var tag in tags)
        {
            // Simple simulation logic
            object? val = null;
            
            if (tag.DataType.Contains("BOOL", StringComparison.OrdinalIgnoreCase))
            {
                val = _random.Next(0, 2) == 1;
            }
            else if (tag.DataType.Contains("INT", StringComparison.OrdinalIgnoreCase))
            {
                val = _random.Next(0, 100);
            }
            else if (tag.DataType.Contains("FLOAT", StringComparison.OrdinalIgnoreCase))
            {
                val = _random.NextDouble() * 100;
            }
            else if (tag.DataType.Contains("STRING", StringComparison.OrdinalIgnoreCase))
            {
                val = $"Sim-{DateTime.Now.Second}";
            }

            result[tag.Name] = val;
        }

        return Task.FromResult(result);
    }

    public Task WriteTagAsync(Tag tag, object value)
    {
        // Simulate write delay
        return Task.Delay(10);
    }

    public void Dispose()
    {
        DisconnectAsync();
    }
}
