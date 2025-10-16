using System.Collections.Generic;
using System.Linq;
using GDS.Core;
using UnityEngine;

namespace GDS.Demos.Basic {
    public static class Bases {


        public static readonly ArmorBase WarriorHelmet = new() { Id = "WarriorHelmet", Name = "Warrior Helmet", Icon = "Shared/Images/items/helmet", Class = Class.Helmet, Defense = new(100, 200) };
        public static readonly ArmorBase LeatherGloves = new() { Id = "LeatherGloves", Name = "Leather Gloves", Icon = "Shared/Images/items/gloves", Class = Class.Gloves, Defense = new(25, 50) };
        public static readonly ArmorBase LeatherArmor = new() { Id = "LeatherArmor", Name = "Leather Armor", Icon = "Shared/Images/items/armor", Class = Class.BodyArmor, Defense = new(120, 140) };
        public static readonly ArmorBase SteelBoots = new() { Id = "SteelBoots", Name = "Steel Boots", Icon = "Shared/Images/items/boots", Class = Class.Boots, Defense = new(30, 60) };

        public static readonly WeaponBase Axe = new() { Id = "Axe", Name = "Axe", Icon = "Shared/Images/items/axe", Class = Class.Axe2h, Attack = new(120, 140), AttackSpeed = 1.2f };
        public static readonly WeaponBase LongSword = new() { Id = "LongSword", Name = "Long Sword", Icon = "Shared/Images/items/sword", Class = Class.Sword1h, Attack = new(90, 120), AttackSpeed = 1.35f };
        public static readonly WeaponBase ShortSword = new() { Id = "ShortSword", Name = "Short Sword", Icon = "Shared/Images/items/sword-blue", Class = Class.Sword1h, Attack = new(80, 100), AttackSpeed = 1.5f };

        public static readonly ItemBase BlueAmulet = new() { Id = "BlueAmulet", Name = "Silver Amulet", Icon = "Shared/Images/items/necklace", Stack = Stack.NoStack, Class = Class.Amulet };
        public static readonly ItemBase GoldRing = new() { Id = "GoldRing", Name = "Gold Ring", Icon = "Shared/Images/items/ring", Stack = Stack.NoStack, Class = Class.Ring };

        public static readonly ItemBase Apple = new() { Id = "Apple", Name = "Apple", Icon = "Shared/Images/items/apple", Stack = Stack.Infinite, Class = Class.Consumable };
        public static readonly ItemBase Mushroom = new() { Id = "Mushroom", Name = "Mushroom", Icon = "Shared/Images/items/mushroom", Stack = Stack.Infinite, Class = Class.Consumable };
        public static readonly ItemBase HealthPotion = new() { Id = "HealthPotion", Name = "Health Potion", Icon = "Shared/Images/items/potion", Stack = Stack.Infinite, Class = Class.Consumable };

        public static readonly ItemBase Wood = new() { Id = "Wood", Name = "Wood", Icon = "Shared/Images/items/wood", Stack = Stack.Infinite, Class = Class.Material };
        public static readonly ItemBase Steel = new() { Id = "Steel", Name = "Steel", Icon = "Shared/Images/items/silver", Stack = Stack.Infinite, Class = Class.Material };
        public static readonly ItemBase Gem = new() { Id = "Gem", Name = "Gem", Icon = "Shared/Images/items/gem", Stack = new(10), Class = Class.Material };

    }

    public static class BasesExt {
        public static readonly IEnumerable<ItemBase> All = typeof(Bases).GetFields().Select(field => (ItemBase)field.GetValue(null));
        public static readonly IEnumerable<ItemBase> NonStackable = All.Where(b => b.Stack is NoStack);

        public static ItemBase RandomBase() => All.ElementAt(Random.Range(0, All.Count()));
        public static ItemBase RandomNoStackBase() => NonStackable.ElementAt(Random.Range(0, NonStackable.Count()));
        public static readonly string[] AllIds = All.Select(b => b.Id).ToArray();
        public static readonly HashSet<string> AllIdsHash = new HashSet<string>(AllIds);
        public static ItemBase Get(string id) => AllIdsHash.Contains(id)
            ? All.FirstOrDefault(b => b.Id == id)
            : ItemBase.NoBase;

    }
}