using System.Linq;
using UnityEngine.UIElements;

namespace GDS.Core.Views {

    /// <summary>
    /// A "smart" component that displays a list of slots.
    /// <para>Requires USS classes: <c>list-bag, slot-container</c></para>
    /// </summary>
    public class ListBagView : VisualElement {
        public ListBagView(ListBag bag, CreateSlotFn createSlotFn = null) {
            createSlotFn ??= slot => new SlotView(bag, slot);
            var slotViews = bag.Slots.Select(slot => createSlotFn(slot)).ToArray();

            this.Add("list-bag", Dom.Div("slot-container", slotViews));
            this.Observe(bag.Data, (slots) => {
                for (var i = 0; i < slots.Count; i++) slotViews[i].Data = slots[i];
            });
        }
    }
}