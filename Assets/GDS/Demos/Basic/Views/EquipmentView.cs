using UnityEngine.UIElements;
using GDS.Core;
using System.Linq;

namespace GDS.Demos.Basic {
    public class EquipmentView : VisualElement {
        public EquipmentView(Equipment bag) {
            BasicSlotView[] slots = bag.Slots.Select(kv => new BasicSlotView(bag, kv.Value).WithClass(kv.Key)).ToArray();

            this.Add("equipment slot-container", slots);
            this.Observe(bag.Data, (data) => {
                foreach (var slot in slots) slot.Data = data[(slot.Slot as SetSlot).Key];
            });
        }

    }
}
