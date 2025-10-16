using System;
using GDS.Core;

namespace GDS.Demos.Basic {
    public static class Filters {
        public static readonly Predicate<Item> Weapon = item => item is WeaponItem;
        public static readonly Predicate<Item> Armor = item => item is WeaponItem;
        public static readonly Predicate<Item> Helmet = item => item.Class() == Class.Helmet;
        public static readonly Predicate<Item> Gloves = item => item.Class() == Class.Gloves;
        public static readonly Predicate<Item> Boots = item => item.Class() == Class.Boots;
        public static readonly Predicate<Item> BodyArmor = item => item.Class() == Class.BodyArmor;
        public static readonly Predicate<Item> Consumable = (item) => item.Class() == Class.Consumable;
        public static readonly Predicate<Item> Material = (item) => item.Class() == Class.Material;
    }
}