using UnityEngine;
using GDS.Core;
using GDS.Core.Events;
using GDS.Common.Events;
using UnityEngine.EventSystems;


namespace GDS.Common.Scripts {


    /// <summary>
    /// A world item is created when you discard items from an inventory (behavior handled in another script). It can be picked up (looted) by clicking on it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldItem : MonoBehaviour, IWorldItem, IHighlight, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] GameObject background;
        SpriteRenderer Renderer;
        Color InitialColor;
        Color HighlightColor = new Color(0.5f, 0.5f, 0.5f);
        Vector3 InitialScale;
        float ScaleFactor = 1.175f;

        Item item;
        public Item Item {
            get => item;
            set {
                item = value;
                Renderer.sprite = Resources.Load<Sprite>(item.Base.Icon);
            }
        }

        public GameObject GameObject => gameObject;

        void Awake() {
            Renderer = GetComponent<SpriteRenderer>();
            InitialColor = Renderer.color;
            InitialScale = Renderer.transform.localScale;
            background.SetActive(false);
        }

        public void Highlight() {
            Renderer.color = HighlightColor;
            Renderer.transform.localScale = InitialScale * ScaleFactor;
            background.SetActive(true);
        }

        public void Unhighlight() {
            Renderer.color = InitialColor;
            Renderer.transform.localScale = InitialScale;
            background.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData) => EventBus.Global.Publish(new LootWorldItem(this));
        public void OnPointerEnter(PointerEventData eventData) => Highlight();
        public void OnPointerExit(PointerEventData eventData) => Unhighlight();

    }
}