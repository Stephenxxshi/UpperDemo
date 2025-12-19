using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class SimulationDriver : IDriver
{
    private bool _isConnected;
    private readonly Random _random = new();

    public bool IsConnected => _isConnected;

    public void Initialize(DeviceConfig config)
    {
        // No config needed for simulation
    }

    public void ValidateConfig(DeviceConfig config)
    {
        // Always valid
    }

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

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        var result = new Dictionary<string, object?>();
        
        if (!_isConnected) return Task.FromResult(result);

        foreach (var tagObj in tags)
        {
            var tag = tagObj as CommunicationTag;
            if (tag == null) continue;
            
            // Simple simulation logic
            object? val = null;
            
            switch (tag.DataType)
            {
                case TagDataType.Boolean:
                    val = _random.Next(0, 2) == 1;
                    break;
                case TagDataType.Int16:
                case TagDataType.Int32:
                case TagDataType.Int64:
                case TagDataType.UInt16:
                case TagDataType.UInt32:
                case TagDataType.UInt64:
                case TagDataType.Byte:
                    val = _random.Next(0, 100);
                    break;
                case TagDataType.Float:
                case TagDataType.Double:
                    val = _random.NextDouble() * 100;
                    break;
                case TagDataType.String:
                    val = $"Sim-{DateTime.Now.Second}";
                    break;
                default:
                    val = 0;
                    break;
            }

            // Handle array
            if (tag.ArrayLength > 1 && tag.DataType != TagDataType.String)
            {
                // Create array
                // For simplicity, just creating an array of the value type
                // Note: val might be int, but DataType is Int16, so we might need casting.
                // But for simulation, let's keep it simple.
                var type = val.GetType();
                var arr = Array.CreateInstance(type, tag.ArrayLength);
                for(int i=0; i<tag.ArrayLength; i++)
                {
                    arr.SetValue(val, i); 
                }
                val = arr;
            }

            result[tag.Name] = val;
        }

        return Task.FromResult(result);
    }

    public Task WriteTagAsync(object tag, object value)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
