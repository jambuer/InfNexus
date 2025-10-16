
using UnityEngine.UIElements;
using GDS.Core;
using GDS.Core.Views;
namespace GDS.Examples.Minimal {
    /// <summary>
    /// The Root Layer is the top-most visual element
    /// </summary>
    public class RootLayer : VisualElement {
        public new class UxmlFactory : UxmlFactory<RootLayer> { }
        public RootLayer() {
            var store = Store.Instance;
            // This declatative call creates the visual tree structure 
            // adds the `root-layer` uss class to current element
            this.Add("root-layer",
                // adds a view that renders the `Main Inventory`
                new ListBagView(store.Main)
            );
            // Add behavior that allows picking (and dragging) items
            this.WithDragToPickBehavior(store.DraggedItem, store.Bus);

            // Add behavior that shows the currently dragged item
            this.WithGhostItemBehavior(store.DraggedItem, new ItemView());

            // Add behavior that highlights the slot under the cursor
            this.WithRestrictedSlotBehavior(store.DraggedItem);

        }
    }
}
