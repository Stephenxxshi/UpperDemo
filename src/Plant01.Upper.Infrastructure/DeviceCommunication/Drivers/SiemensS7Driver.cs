using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class SiemensS7Driver : IDriver
{
    public bool IsConnected => throw new NotImplementedException();

    public Task ConnectAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<Tag> tags)
    {
        throw new NotImplementedException();
    }

    public Task WriteTagAsync(Tag tag, object value)
    {
        throw new NotImplementedException();
    }
}
