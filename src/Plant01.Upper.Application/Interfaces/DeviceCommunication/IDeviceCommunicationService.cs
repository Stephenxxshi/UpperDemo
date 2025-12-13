using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Application.Interfaces.DeviceCommunication;

public interface IDeviceCommunicationService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    TagData GetTagValue(string tagName);
    T GetTagValue<T>(string tagName, T defaultValue = default);
    Task WriteTagAsync(string tagName, object value);
    
    event EventHandler<TagChangeEventArgs>? TagChanged;
}

public class TagChangeEventArgs : EventArgs
{
    public string TagName { get; }
    public TagData NewData { get; }

    public TagChangeEventArgs(string tagName, TagData newData)
    {
        TagName = tagName;
        NewData = newData;
    }
}
