namespace Plant01.Core.Components;

public interface IMessenger
{
    void Register<TMessage>(Action<TMessage> handler, object token = null);
    void Unregister<TMessage>(Action<TMessage> handler, object token = null);
    void Send<TMessage>(TMessage message, object token = null);

    // 新增请求/响应
    void Register<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null);
    void Unregister<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null);
    TResponse Send<TRequest, TResponse>(TRequest request, object token = null);
}
