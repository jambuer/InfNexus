using UnityEngine;
using UnityEngine.EventSystems;

namespace GDS.Common.Scripts {

    /// <summary>
    /// Highlights an object on mouse over (changes object material).
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class HighlightObject : MonoBehaviour, IHighlight, IPointerEnterHandler, IPointerExitHandler {
        Renderer Renderer;
        Material Initial;
        [SerializeField] Material HighlightMaterial;

        void Awake() {
            Renderer = GetComponent<Renderer>();
            Initial = Renderer.material;
        }

        public void Highlight() => Renderer.material = HighlightMaterial;
        public void Unhighlight() => Renderer.material = Initial;

        public void OnPointerEnter(PointerEventData eventData) => Highlight();
        public void OnPointerExit(PointerEventData eventData) => Unhighlight();
    }
}