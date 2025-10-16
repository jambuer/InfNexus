using UnityEngine.UIElements;
using GDS.Core.Views;
using GDS.Core;
using GDS.Demos.Basic;
using UnityEngine;

namespace GDS.Demos.Basic.Views {
    public class BasicItemView : Component<Item> {

        public BasicItemView() {

            this.Add("item",
                bg.WithClass("item-background"),
                image.WithClass("item-image"),
                quant.WithClass("item-quant"),
                debug.WithClass("debug-label")
            ).PickIgnoreAll();
        }

        VisualElement bg = new();
        VisualElement image = new();
        Label quant = new();
        Label debug = new();

        override public void Render(Item data) {
            if (data is not BasicItem item) { this.Hide(); return; }

            // Debug.Log(item.Base);
            this.Show();
            // debug.text = $"[{item.Name}]\n[{item.Rarity}]";
            debug.text = DebugText(item);
            bg.ClearClassList();
            bg.WithClass("item-background", item.Rarity.ToString());
            image.style.backgroundImage = new StyleBackground(item.Image());
            quant.SetVisible(item.Stackable).text = item.Quant.ToString();
        }

        public string DebugText(BasicItem item) => item switch {
            ArmorItem i => $"[{item.Name()}]\n[{item.Rarity}]\nDefense: {i.Defense}",
            WeaponItem i => $"[{item.Name()}]\n[{item.Rarity}]\nDPS: {i.DPS}",
            _ => $"[{item.Base.Name}]\n[{item.Rarity}]"
        };


    }
}