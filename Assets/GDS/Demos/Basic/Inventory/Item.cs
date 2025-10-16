using GDS.Core;

namespace GDS.Demos.Basic {

    public record BasicItem : Item {
        public Rarity Rarity = Rarity.NoRarity;
        public string CustomName { get; init; }
        public string CustomIcon { get; init; }
    }

    public record WeaponItem : BasicItem {
        public WeaponItem(WeaponBase itemBase) { Base = itemBase; Attack = itemBase.Attack.Roll(); }
        public int Attack;
        public float AttackSpeed => (Base as WeaponBase).AttackSpeed;
        public float DPS => Attack * AttackSpeed;
    }

    public record ArmorItem : BasicItem {
        public ArmorItem(ArmorBase itemBase) { Base = itemBase; Defense = itemBase.Defense.Roll(); }
        public int Defense;
    }

    public static class ItemFactory {
        public static BasicItem CreateRandom() => BasesExt.RandomBase().Create(CommonExt.RandomRarity(), 20);
        public static BasicItem Create(this ItemBase itemBase) => Create(itemBase, Rarity.NoRarity);
        public static BasicItem Create(this ItemBase itemBase, int Quant) => Create(itemBase, Rarity.NoRarity, Quant);
        public static BasicItem Create(this ItemBase itemBase, Rarity rarity, int Quant = 0) => itemBase switch {
            ArmorBase b => new ArmorItem(b) { Rarity = rarity },
            WeaponBase b => new WeaponItem(b) { Rarity = rarity },
            _ => new BasicItem() { Base = itemBase, Rarity = rarity, Quant = System.Math.Min(Quant, itemBase.Stack.Max) }
        };
    }

    public static class ItemExt {
        public static string Name(this BasicItem item) => item.Rarity == GDS.Demos.Basic.Rarity.Unique ? item.CustomName : item.Name;
        public static int Cost(this Item item) => item.Stackable ? item.Quant * 2 : 20;
        public static int SellValue(this Item item) => item.Cost() / 2;
        public static Rarity Rarity(this Item item) => item is BasicItem i ? i.Rarity : GDS.Demos.Basic.Rarity.NoRarity;
        public static Class Class(this Item item) => item.Base is ItemBase b ? b.Class : GDS.Demos.Basic.Class.NoClass;
    }

}