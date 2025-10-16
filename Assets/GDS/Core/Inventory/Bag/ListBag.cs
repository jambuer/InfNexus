using System;
using System.Collections.Generic;
using System.Linq;
using GDS.Core.Events;
using UnityEngine;

namespace GDS.Core {

    /// <summary>
    /// Represents an inventory as a flat list of items. 
    /// Items are added in order, and each occupies a single slot without spatial constraints.
    /// </summary>
    public class ListBag : Bag {
        public Observable<List<ListSlot>> Data;
        public List<ListSlot> Slots => Data.Value;
        public List<Item> Items => Slots.Where(slot => slot.IsFull()).Select(slot => slot.Item).ToList();
        public int Size => Slots.Count;
        virtual public void Init() { }
    }

    public static class ListBagFactory {

        public static T Create<T>(string id, int size, Predicate<Item> accepts = null, bool allowStacking = true) where T : ListBag {
            var bag = Activator.CreateInstance<T>();
            var list = Enumerable.Range(0, size).Select(i => new ListSlot(i, Item.NoItem)).ToList();
            bag.Id = id;
            bag.Data = new(list);
            if (accepts != null) bag.Accepts = accepts;
            bag.AllowStacking = allowStacking;
            bag.Init();
            return bag;
        }
    }

    public static class ListBagExt {

        public static bool IsEmpty(this ListBag bag) => bag.Slots.All(slot => slot.IsEmpty());
        public static bool IsFull(this ListBag bag) => bag.Slots.All(slot => slot.IsFull());
        public static int FindEmptyIndex(this ListBag bag) => bag.Slots.FindIndex(slot => slot.IsEmpty());
        public static void Clear(this ListBag bag) { for (int i = 0; i < bag.Slots.Count; i++) bag.Clear(i); }
        public static void Clear(this ListBag bag, int index) => bag.Slots[index] = bag.Slots[index].Clear();



        public static ListBag SetState(this ListBag bag, params Item[] items) {
            bag.Clear();
            bag.AddItems(items);
            return bag;
        }

        public static ListBag SetState(this ListBag bag, params ListSlot[] items) {
            bag.Clear();
            foreach (var item in items) {
                if (item.Index < 0 || item.Index >= bag.Size) continue;
                bag.Slots[item.Index] = bag.Slots[item.Index] with { Item = item.Item };
            }
            bag.Data.Notify();
            return bag;
        }

        public static void SetEmptyState(this ListBag bag) {
            bag.Clear();
            bag.Data.Notify();
        }

        public static Result AddItems(this ListBag bag, params Item[] items) {
            if (bag.IsFull()) return new BagFull(bag);
            var itemsAddedResult = items.Select(bag.AddItem).ToList();
            var itemsThatDontFit = items.Where((_, index) => itemsAddedResult[index] is Fail);
            bag.Data.Notify();
            if (itemsThatDontFit.Count() > 0) return new CantFitAll(bag, itemsThatDontFit);
            return Result.Success;
        }

        public static Result RemoveItem(this ListBag bag, ListSlot slot) {
            UpdateSlot(bag, slot, Item.NoItem);
            return Result.Success;
        }

        public static Result PickItem(this ListBag bag, ListSlot slot, EventModifiers mods = EventModifiers.None) {
            if (mods.Shift() && bag.AllowStacking && slot.Item.Stackable) return UnstackHalf(bag, slot);

            UpdateSlot(bag, slot, Item.NoItem);
            return new PickItemSuccess(slot.Item);
        }

        public static Result PlaceItem(this ListBag bag, ListSlot slot, Item item, EventModifiers mods = EventModifiers.None) {
            if (!bag.Accepts(item)) return Result.Fail;
            if (!slot.Accepts(item)) return Result.Fail;
            if (mods.Shift() && bag.AllowStacking && !item.CanStack(slot.Item)) return Result.Fail;
            if (mods.Shift() && bag.AllowStacking && item.CanStack(slot.Item)) return StackOne(item, bag, slot);
            if (bag.AllowStacking && item.CanStack(slot.Item)) return StackAll(item, bag, slot);

            UpdateSlot(bag, slot, item);
            return new PlaceItemSuccess(item, slot.Item);
        }

        public static void UpdateSlot(this ListBag bag, ListSlot slot, Item item) {
            bag.Slots[slot.Index] = bag.Slots[slot.Index] with { Item = item };
            bag.Data.Notify();
        }

        public static Result SortBag(this ListBag bag, SortFn fn, bool desc) {
            var items = desc
                ? bag.Items.OrderByDescending(i => (dynamic)fn(i))
                : bag.Items.OrderBy(i => (dynamic)fn(i));
            items = items.ThenBy(i => i.Name).ThenByDescending(i => i.Quant);
            bag.SetState(items.ToArray());
            return new Success();
        }

        /* Private */

        static Result AddItem(this ListBag bag, Item item) {
            var index = bag.FindEmptyIndex();
            if (index == -1) return Result.Fail;
            bag.Slots[index] = bag.Slots[index] with { Item = item };
            return Result.Success;
        }


        static Result StackAll(Item item, ListBag bag, ListSlot slot) {
            var (draggedItem, slotItem) = ItemExt.StackAll(item, slot.Item);
            UpdateSlot(bag, slot, slotItem);
            return new StackItemSuccess(item, draggedItem);
        }

        static Result StackOne(Item item, ListBag bag, ListSlot slot) {
            var (fromItem, toItem) = ItemExt.StackOne(item, slot.Item);
            UpdateSlot(bag, slot, toItem);
            return new StackItemSuccess(item with { Quant = 1 }, fromItem);
        }

        static Result UnstackHalf(ListBag bag, ListSlot slot) {
            var (oldItem, newItem) = ItemExt.UnstackHalf(slot.Item);
            UpdateSlot(bag, slot, oldItem);
            return new UnstackItemSuccess(newItem, oldItem);
        }

    }

}