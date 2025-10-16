using GDS.Core.Events;
using UnityEngine;

namespace GDS.Common.Scripts {
    /// <summary>
    /// Disables a Collider when UI is open
    /// </summary>
    public class DisableColliderOnUiOpen : MonoBehaviour {
        public Collider Collider;

        void OnEnable() => EventBus.Global.On<UiStateChanged>(OnUiStateChange);
        void OnDisable() => EventBus.Global.Off<UiStateChanged>(OnUiStateChange);

        void OnUiStateChange(CustomEvent ev) {
            if (ev is not UiStateChanged e) return;
            if (Collider) Collider.enabled = !e.Open;
        }
    }
}