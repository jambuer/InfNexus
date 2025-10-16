using System;

namespace GDS.Core {
    /// <summary>
    /// Represents an item. 
    /// Each item has an ID, an associated base definition, quantity, and derived properties such as name, icon, and stack behavior.
    /// </summary>
    public record Item {
        public static NoItem NoItem = new();
        public Item() => Id = ItemFactory.Id();
        public IItemBase Base = ItemBase.NoItemBase;
        public int Id = -1;
        public int Quant = 0;
        public string Name => Base.Name;
        public string Icon => Base.Icon;
        public bool Stackable => Base.Stack is not NoStack;
        public ushort MaxStack => Base.Stack.Max;
    }

    /// <summary>
    /// Represents the absence of an item. 
    /// </summary>
    public record NoItem : Item;

    /// <summary>
    /// Represents an item an inventory.
    /// Holds references to the bag, the slot, and the item stored there.
    /// </summary>
    public record BagItem(Bag Bag, Slot Slot, Item Item) { public static BagItem NoItem = new(Bag.NoBag, Slot.NoSlot, Item.NoItem); };

    public static class ItemFactory {
        static int id = -1;
        public static int Id() => ++id;
        public static Item Create(this ItemBase itemBase) => itemBase switch {
            _ => new Item() { Base = itemBase }
        };
    }

    public static class ItemExt {
        public static Item Clone(this Item item) => item with { Id = ItemFactory.Id() };
        public static Item SetQuant(this Item item, int quant) => quant <= 0 ? Item.NoItem : item with { Quant = Math.Min(quant, item.MaxStack) };
        public static Item Decrement(this Item item) => item.SetQuant(item.Quant - 1);
        public static bool CanStack(this Item fromItem, Item toItem) => fromItem.Stackable && (toItem is NoItem || (fromItem.Base == toItem.Base && toItem.Quant < toItem.MaxStack));
        public static (Item from, Item to) StackAll(Item fromItem, Item toItem) {
            var total = toItem.Quant + fromItem.Quant;
            var remaining = total > fromItem.MaxStack ? total % fromItem.MaxStack : 0;
            var newToItem = toItem is NoItem ? fromItem : toItem.SetQuant(toItem.Quant + fromItem.Quant);
            var newFromItem = fromItem.SetQuant(remaining);
            return (newFromItem, newToItem);
        }
        public static (Item from, Item to) StackOne(Item fromItem, Item toItem) {
            var newFromItem = fromItem.SetQuant(fromItem.Quant - 1);
            var newToItem = toItem is NoItem ? fromItem.Clone().SetQuant(1) : toItem.SetQuant(toItem.Quant + 1);
            return (newFromItem, newToItem);
        }
        public static (Item from, Item to) UnstackHalf(Item fromItem) {
            int half = fromItem.Quant / 2;
            return (fromItem.SetQuant(half), fromItem.Clone().SetQuant(fromItem.Quant - half));
        }

    }
}