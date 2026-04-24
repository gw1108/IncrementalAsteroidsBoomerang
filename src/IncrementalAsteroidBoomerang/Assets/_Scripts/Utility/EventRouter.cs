using System;
using System.Collections.Generic;

namespace _Scripts.Utility
{
    /// <summary>
    /// Any object can implement this interface to support localized events.
    /// </summary>
    public interface IEventRouterHolder
    {
        public EventRouter EventRouter { get; }
    }
    
    public class EventRouter
    {
        /// <summary>
        /// There will always be a global event router. This is how you access it.
        /// </summary>
        public static EventRouter Instance
        { 
            get { return _instance ??= new EventRouter(); }
        }
        private static EventRouter _instance;
        
        private readonly Dictionary<Type, Delegate> eventListeners = new Dictionary<Type, Delegate>();

        public void Register<TEvent>(Action<TEvent> callback) where TEvent : struct
        {
            Type eventType = typeof(TEvent);
        
            if (eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = Delegate.Combine(eventListeners[eventType], callback);
            }
            else
            {
                eventListeners[eventType] = callback;
            }
        }

        public void Unregister<TEvent>(Action<TEvent> callback) where TEvent : struct
        {
            Type eventType = typeof(TEvent);
        
            if (eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = Delegate.Remove(eventListeners[eventType], callback);
            
                if (eventListeners[eventType] == null)
                {
                    eventListeners.Remove(eventType);
                }
            }
        }

        public void Broadcast<TEvent>(TEvent eventData) where TEvent : struct
        {
            Type eventType = typeof(TEvent);
        
            if (eventListeners.TryGetValue(eventType, out var listener))
            {
                Action<TEvent> callback = listener as Action<TEvent>;
                callback?.Invoke(eventData);
            }
        }
    }
}
