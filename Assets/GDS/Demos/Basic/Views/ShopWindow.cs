using UnityEngine.UIElements;
using GDS.Core;
using GDS.Core.Views;
using GDS.Demos.Basic.Views;
using GDS.Demos.Basic.Events;
using static GDS.Core.Dom;

namespace GDS.Demos.Basic {

    public class ShopWindow : VisualElement {

        public ShopWindow(Shop bag) {

            this.Add("window vendor",
                Title("Shop"),
                new ListBagView(bag, BasicSlotView.Create(bag)),
                Div("row", Button("Reroll Shop", bag.Reroll)),
                Components.CloseButton(bag)
            );
        }
    }
}
