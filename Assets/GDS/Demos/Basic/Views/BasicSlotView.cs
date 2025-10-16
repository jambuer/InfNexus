using GDS.Core;
using GDS.Core.Views;
using GDS.Demos.Basic.Views;

namespace GDS.Demos.Basic {
    public class BasicSlotView : SlotView {
        public BasicSlotView(Bag bag, Slot slot) : base(bag, slot, () => new BasicItemView()) { }
        public static CreateSlotFn Create(Bag bag) => slot => new BasicSlotView(bag, slot);
    }
}