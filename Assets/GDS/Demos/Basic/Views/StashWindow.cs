using UnityEngine.UIElements;
using GDS.Core;
using GDS.Demos.Basic.Views;

namespace GDS.Demos.Basic {
    public class StashWindow : VisualElement {
        public StashWindow(Stash bag) {
            this.Add("window",
                Components.CloseButton(bag),
                Dom.Div("row stretch justify-space-between",
                    Dom.Title("Stash"),
                    Components.SortBy(bag, Store.Bus)
                ),
                new BasicListView(bag)
            );
        }
    }
}
