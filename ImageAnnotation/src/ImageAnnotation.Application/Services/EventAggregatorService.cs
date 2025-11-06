namespace ImageAnnotation.Application.Services;

/// <summary>
/// Simple event aggregator for loosely coupled communication.
/// </summary>
public class EventAggregatorService
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    /// <summary>
    /// Subscribes to an event.
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);

        if (!_subscribers.ContainsKey(eventType))
            _subscribers[eventType] = new List<Delegate>();

        _subscribers[eventType].Add(handler);
    }

    /// <summary>
    /// Publishes an event.
    /// </summary>
    public void Publish<TEvent>(TEvent eventData)
    {
        var eventType = typeof(TEvent);

        if (!_subscribers.ContainsKey(eventType))
            return;

        foreach (var handler in _subscribers[eventType].Cast<Action<TEvent>>())
        {
            handler(eventData);
        }
    }

    /// <summary>
    /// Unsubscribes from an event.
    /// </summary>
    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);

        if (_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType].Remove(handler);
        }
    }
}
