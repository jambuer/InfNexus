using System;

namespace GDS.Core {
    // Q: Why not put a Bag reference here?
    // A: Because of Record's value type semantics - The reference will be pointing to a potentially old value of the bag
    /// <summary>
    /// A Slot is an Item container. Has a predicate function that checks if it accepts an Item
    /// </summary>
    public record Slot(Item Item) {
        public static Slot NoSlot = new(Item.NoItem);
        public Predicate<Item> Accepts = _ => true;
    };
    /// <summary>
    /// A slot with an index, used in inventories where slot order matters
    /// </summary>
    public record ListSlot(int Index, Item Item) : Slot(Item);
    /// <summary>
    /// A slot with a name, used in inventories like Equipment
    /// </summary>    
    public record SetSlot(string Key, Item Item) : Slot(Item);

    public static class SlotFactory {
        public static ListSlot Create(int index) => new(index, Item.NoItem);
        public static SetSlot Create(string key) => new SetSlot(key, Item.NoItem);
        public static SetSlot Create(string key, Predicate<Item> accepts) => new SetSlot(key, Item.NoItem) { Accepts = accepts };
    }

    public static class SlotExt {
        public static bool IsEmpty(this Slot slot) => slot.Item == Item.NoItem;
        public static bool IsFull(this Slot slot) => slot.Item != Item.NoItem;
        public static T Clear<T>(this T slot) where T : Slot => slot with { Item = Item.NoItem };
    }
}