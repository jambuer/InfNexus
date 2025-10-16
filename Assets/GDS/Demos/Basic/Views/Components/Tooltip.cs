using GDS.Core;
using GDS.Core.Views;
using UnityEngine.UIElements;
using static GDS.Core.Dom;

namespace GDS.Demos.Basic.Views {

    public class Tooltip : Component<BagItem> {

        public override void Render(BagItem data) {
            Clear();
            ClearClassList();
            this.Add($"tooltip {data.Item.Rarity()}", CreateTooltip(data));
        }

        VisualElement CreateTooltip(BagItem bi) => (bi.Item, bi.Slot) switch {
            (_, CraftOutcomeSlot) => CraftTooltip(bi.Item),
            (WeaponItem i, _) => WeaponTooltip(i, CostText(bi)),
            (ArmorItem i, _) => ArmorTooltip(i, CostText(bi)),
            (BasicItem i, _) => DefaultTooltip(i, CostText(bi)),
            _ => Label("unknown item type")
        };

        VisualElement CraftTooltip(Item item) => Div(
            Label("tooltip-item-name", $"Random {item.Name}")
        );

        VisualElement WeaponTooltip(WeaponItem item, string cost) => Div(
           NameLabel(item),
           RarityLabel(item),
           AffixLabel("Attack", item.Attack),
           AffixLabel("Attack Speed", item.AttackSpeed),
           AffixLabel("DPS", item.DPS),
           CostLabel(cost)
       );

        VisualElement ArmorTooltip(ArmorItem item, string cost) => Div(
            NameLabel(item),
            RarityLabel(item),
            AffixLabel("Defense", item.Defense),
            CostLabel(cost)
        );

        VisualElement DefaultTooltip(BasicItem item, string cost) => Div(
            NameLabel(item),
            RarityLabel(item),
            CostLabel(cost)
        );

        Label NameLabel(BasicItem item) => Label("tooltip-item-name", item.Stackable ? $"{item.Name} ({item.Quant})" : item.Name);
        Label RarityLabel(BasicItem item) => Label("tooltip-affix-text", $"[{item.Rarity}]").SetVisible(item.Rarity is not Rarity.NoRarity);
        Label AffixLabel(string label, object value) => Label("tooltip-affix-text", $"{label}: " + ColorUtil.Bold(value));
        Label CostLabel(string cost) => Label("tooltip-item-cost", cost);

        string CostText(BagItem bi) => bi.Bag switch {
            Shop => "Cost: " + ColorUtil.Bold(bi.Item.Cost()),
            _ => "Sell Value: " + ColorUtil.Bold(bi.Item.SellValue())
        };

    }

}