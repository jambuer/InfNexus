using UnityEngine.UIElements;
using GDS.Core;
using GDS.Demos.Basic.Views;
using static GDS.Core.Dom;

namespace GDS.Demos.Basic {

    public class InventoryView : VisualElement {

        public InventoryView() {
            var store = Store.Instance;
            var bus = Store.Bus;

            this.Add("window",
                Components.CloseButton(store.Main),
                Gold(),
                Div(
                    Title("Equipment (Equipment only)"),
                    new EquipmentView(store.Equipment)),
                Div(
                    Div("row justify-space-between",
                        Title("Inventory (Unrestricted)"),
                        new SortByComponent(store.Main, bus)
                    ),
                    new BasicListView(store.Main)),
                Div(
                    Title("Hotbar (Consumables only)"),
                    new BasicListView(store.Hotbar))
            ).Gap(50);
        }

        VisualElement Gold() {
            var label = Label("player-gold-label", "");
            var icon = Div("player-gold-icon");
            var el = Div("player-gold", icon, label);
            el.Observe(Store.Instance.Gold, value => label.text = value.ToString());
            return el;
        }

    }

}
