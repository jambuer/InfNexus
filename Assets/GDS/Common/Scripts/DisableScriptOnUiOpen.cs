using GDS.Core.Events;
using UnityEngine;

namespace GDS.Common.Scripts {
    /// <summary>
    /// Disables a Behavior when UI is open
    /// </summary>
    public class DisableScriptOnUiOpen : MonoBehaviour {
        public Behaviour Script;

        void OnEnable() => EventBus.Global.On<UiStateChanged>(OnUiStateChange);
        void OnDisable() => EventBus.Global.Off<UiStateChanged>(OnUiStateChange);

        void OnUiStateChange(CustomEvent ev) {
            if (ev is not UiStateChanged e) return;
            if (Script) Script.enabled = !e.Open;
        }
    }
}