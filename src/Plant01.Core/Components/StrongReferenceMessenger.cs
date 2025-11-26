namespace Plant01.Core.Components;

/// <summary>
/// 强引用版本，消息中心会持有订阅者的强引用，需要手动取消订阅
/// </summary>
public sealed class StrongReferenceMessenger : IMessenger
{
    private static readonly Lazy<StrongReferenceMessenger> _default =
        new(() => new StrongReferenceMessenger());

    public static StrongReferenceMessenger Default => _default.Value;

    // 普通消息
    private readonly Dictionary<(Type, object), List<Delegate>> _subscribers = new();

    // 请求/响应消息
    private readonly Dictionary<(Type, object), List<Delegate>> _requestHandlers = new();

    private StrongReferenceMessenger() { }

    public void Register<TMessage>(Action<TMessage> handler, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (!_subscribers.TryGetValue(key, out var handlers))
        {
            handlers = new List<Delegate>();
            _subscribers[key] = handlers;
        }
        handlers.Add(handler);
    }

    public void Unregister<TMessage>(Action<TMessage> handler, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (_subscribers.TryGetValue(key, out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    public void Send<TMessage>(TMessage message, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (_subscribers.TryGetValue(key, out var handlers))
        {
            foreach (var handler in handlers.ToList())
            {
                ((Action<TMessage>)handler)(message);
            }
        }
    }

    // ----------- Request/Reply -----------
    public void Register<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (!_requestHandlers.TryGetValue(key, out var handlers))
        {
            handlers = new List<Delegate>();
            _requestHandlers[key] = handlers;
        }
        handlers.Add(handler);
    }

    public void Unregister<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (_requestHandlers.TryGetValue(key, out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    public TResponse Send<TRequest, TResponse>(TRequest request, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (_requestHandlers.TryGetValue(key, out var handlers))
        {
            // 默认取第一个响应
            var handler = handlers.FirstOrDefault();
            if (handler is Func<TRequest, TResponse> func)
            {
                return func(request);
            }
        }
        return default!;
    }
}


