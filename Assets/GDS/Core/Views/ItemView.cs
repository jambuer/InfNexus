using UnityEngine.UIElements;

namespace GDS.Core.Views {

    public delegate Component<Item> CreateItemFn();

    /// <summary>
    /// A component that displays an item and it's quantity (if aplicable)
    /// Has pointer events disabled
    /// </summary>
    /// 
    public class ItemView : Component<Item> {

        public ItemView() {
            this.Add("item",
                image.WithClass("item-image"),
                quant.WithClass("item-quant"),
                debug.WithClass("debug-label")
            ).PickIgnoreAll();
        }

        VisualElement image = new();
        Label quant = new();
        Label debug = new();

        override public void Render(Item item) {
            debug.text = $"[{item.Name}]";
            image.style.backgroundImage = new StyleBackground(item.Image());
            quant.text = item.Quant.ToString();
            quant.SetVisible(item.Stackable);
        }
    }
}