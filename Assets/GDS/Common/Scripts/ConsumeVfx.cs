using UnityEngine;
using GDS.Core.Events;

namespace GDS.Demos.Basic {
    /// <summary>
    /// Create a particle effect when consuming items. 
    /// </summary>
    public class ConsumeVfx : MonoBehaviour {
        [SerializeField] GameObject ConsumeVFX;

        private void OnEnable() {
            EventBus.Global.On<ConsumeItemSuccess>(OnConsumeItemSuccess);
        }

        private void OnDisable() {
            EventBus.Global.Off<ConsumeItemSuccess>(OnConsumeItemSuccess);
        }

        void OnConsumeItemSuccess(CustomEvent e) {
            var pos = transform.position;
            pos.y = 2f;
            Instantiate(ConsumeVFX, pos, Quaternion.identity);
        }
    }
}