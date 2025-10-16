using UnityEngine;

namespace GDS.Core {
    public static class EventUtil {
        public static bool Ctrl(this EventModifiers modifiers) => modifiers.HasFlag(EventModifiers.Control);
        public static bool Shift(this EventModifiers modifiers) => modifiers.HasFlag(EventModifiers.Shift);
    }
}