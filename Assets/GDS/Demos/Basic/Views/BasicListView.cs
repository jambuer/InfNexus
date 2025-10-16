using GDS.Core;
using GDS.Core.Views;

namespace GDS.Demos.Basic {
    public class BasicListView : ListBagView {
        public BasicListView(ListBag bag) : base(bag, BasicSlotView.Create(bag)) { }
    }
}