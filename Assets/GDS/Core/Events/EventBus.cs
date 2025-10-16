using System;
using System.Collections.Generic;

namespace GDS.Core.Events {
    /// <summary>
    /// <para>EventBus defines a channel on which events can be published.</para>
    /// <para>It notifies Subscribers when an event of type T has been published.</para>
    /// </summary>
    public class EventBus {
        public static readonly EventBus Global = new();

        public Dictionary<Type, List<Action<CustomEvent>>> ByType = new();
        public Dictionary<Type, List<Action<CustomEvent>>> ByAnyType = new();

        public void On<T>(Action<CustomEvent> eventHandler) where T : CustomEvent {
            var type = typeof(T);
            if (!ByType.TryGetValue(type, out var Onrs)) ByType.Add(type, new());
            ByType[type].Add(eventHandler);
        }

        public void Off<T>(Action<CustomEvent> eventHandler) where T : CustomEvent {
            if (ByType.TryGetValue(typeof(T), out var Onrs)) Onrs.Remove(eventHandler);
        }

        public void OnAny<T>(Action<CustomEvent> eventHandler) where T : CustomEvent {
            var type = typeof(T);
            if (!ByAnyType.TryGetValue(type, out var Onrs)) ByAnyType.Add(type, new());
            ByAnyType[type].Add(eventHandler);
        }

        public void OffAny<T>(Action<CustomEvent> eventHandler) where T : CustomEvent {
            if (ByAnyType.TryGetValue(typeof(T), out var Onrs)) Onrs.Remove(eventHandler);
        }

        public void Publish(CustomEvent Event) {
            // A `NoEvent` will never get published
            if (Event is NoEvent) return;

            var eventType = Event.GetType();
            if (ByType.TryGetValue(eventType, out var subs)) subs.ForEach(s => s.Invoke(Event));

            // NOTE: This needs some profiling, might be a costly reflection operation
            foreach (var entry in ByAnyType) {
                var eventKey = entry.Key;

                if (eventKey.IsAssignableFrom(eventType)) {
                    foreach (var sub in entry.Value) {
                        sub.Invoke(Event);
                    }
                }
            }
        }

    }

}