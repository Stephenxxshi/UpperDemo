namespace Plant01.Core.Components;

/// <summary>
/// 弱引用版本，消息中心仅持有 WeakReference，不需要手动取消订阅
/// </summary>
public sealed class WeakReferenceMessenger : IMessenger
{
    private static readonly Lazy<WeakReferenceMessenger> _default =
        new(() => new WeakReferenceMessenger());

    public static WeakReferenceMessenger Default => _default.Value;

    private readonly Dictionary<(Type, object), List<WeakReference<Delegate>>> _subscribers = new();
    private readonly Dictionary<(Type, object), List<WeakReference<Delegate>>> _requestHandlers = new();

    private WeakReferenceMessenger() { }

    public void Register<TMessage>(Action<TMessage> handler, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (!_subscribers.TryGetValue(key, out var handlers))
        {
            handlers = new List<WeakReference<Delegate>>();
            _subscribers[key] = handlers;
        }
        handlers.Add(new WeakReference<Delegate>(handler));
    }

    public void Unregister<TMessage>(Action<TMessage> handler, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (_subscribers.TryGetValue(key, out var handlers))
        {
            var dead = handlers
                .Where(w => w.TryGetTarget(out var target) && target == (Delegate)handler)
                .ToList();
            foreach (var d in dead) handlers.Remove(d);
        }
    }

    public void Send<TMessage>(TMessage message, object token = null)
    {
        var key = (typeof(TMessage), token);
        if (_subscribers.TryGetValue(key, out var handlers))
        {
            var dead = new List<WeakReference<Delegate>>();
            foreach (var weak in handlers)
            {
                if (weak.TryGetTarget(out var target))
                {
                    ((Action<TMessage>)target)(message);
                }
                else
                {
                    dead.Add(weak);
                }
            }
            foreach (var d in dead) handlers.Remove(d);
        }
    }

    // ----------- Request/Reply -----------
    public void Register<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (!_requestHandlers.TryGetValue(key, out var handlers))
        {
            handlers = new List<WeakReference<Delegate>>();
            _requestHandlers[key] = handlers;
        }
        handlers.Add(new WeakReference<Delegate>(handler));
    }

    public void Unregister<TRequest, TResponse>(Func<TRequest, TResponse> handler, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (_requestHandlers.TryGetValue(key, out var handlers))
        {
            var dead = handlers
                .Where(w => w.TryGetTarget(out var target) && target == (Delegate)handler)
                .ToList();
            foreach (var d in dead) handlers.Remove(d);
        }
    }

    public TResponse Send<TRequest, TResponse>(TRequest request, object token = null)
    {
        var key = (typeof((TRequest, TResponse)), token);
        if (_requestHandlers.TryGetValue(key, out var handlers))
        {
            foreach (var weak in handlers)
            {
                if (weak.TryGetTarget(out var target))
                {
                    if (target is Func<TRequest, TResponse> func)
                        return func(request);
                }
            }
        }
        return default!;
    }
}
