using UnityEngine;

namespace GDS.Demos.Basic {
    public enum BagType { Chest, Stash, Shop };
    public enum Rarity { NoRarity, Common, Magic, Rare, Unique }
    public enum Class { NoClass, Helmet, Gloves, Boots, BodyArmor, Sword1h, Axe2h, Consumable, Material, Amulet, Ring }
    public record Range(int Min = 0, int Max = 0);

    public static class CommonExt {
        public static int Roll(this Range affix) => new System.Random().Next(affix.Min, affix.Max + 1);
        public static Rarity RandomRarity() => (Rarity)Random.Range(1, System.Enum.GetValues(typeof(Rarity)).Length);
    }
}