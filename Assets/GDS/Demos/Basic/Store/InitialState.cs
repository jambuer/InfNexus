using GDS.Core;

namespace GDS.Demos.Basic {
    public static class InitialState {

        public static SetSlot[] Equip = new SetSlot[] {
            new(Equipment.Keys.Helmet, Bases.WarriorHelmet.Create(Rarity.Common)),
            new(Equipment.Keys.Gloves, Bases.LeatherGloves.Create(Rarity.Magic)),
            new(Equipment.Keys.BodyArmor, Bases.LeatherArmor.Create(Rarity.Rare)),
            new(Equipment.Keys.Boots, Bases.SteelBoots.Create(Rarity.Unique)),
            new(Equipment.Keys.Weapon, Bases.LongSword.Create(Rarity.Magic)),
        };

        public static BasicItem[] Main = new[] {
            Bases.Apple.Create(100),
            Bases.Apple.Create(50),
            Bases.Wood.Create(100),
            Bases.Wood.Create(50),
            Bases.Steel.Create(100),
            Bases.Steel.Create(50),
            Bases.Gem.Create(100),
            Bases.Gem.Create(50),
            Bases.WarriorHelmet.Create(Rarity.Unique),
            Bases.ShortSword.Create(Rarity.Magic),
        };

        public static BasicItem[] Hotbar = new[] {
            Bases.HealthPotion.Create(20)
        };

        public static BasicItem[] Shop = new[] {
            Bases.Axe.Create(Rarity.Rare),
            Bases.WarriorHelmet.Create(Rarity.Magic),
            Bases.LongSword.Create(Rarity.Unique),
            Bases.LeatherArmor.Create(Rarity.Rare),
            Bases.LeatherGloves.Create(Rarity.Magic),
            Bases.SteelBoots.Create(Rarity.Unique),
            Bases.Wood.Create(100),
            Bases.Apple.Create(100),
            Bases.Steel.Create(100),
        };

        public static BasicItem[] Chest = new[] {
            Bases.Axe.Create(Rarity.Rare),
            Bases.LongSword.Create(Rarity.Unique),
            Bases.LeatherArmor.Create(Rarity.Rare),
            Bases.LeatherGloves.Create(Rarity.Magic),
            Bases.SteelBoots.Create(Rarity.Unique),
            Bases.Wood.Create(100),
            Bases.Apple.Create(100),
            Bases.Steel.Create(100),
        };



    }
}