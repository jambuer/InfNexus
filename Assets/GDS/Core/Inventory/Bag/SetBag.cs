using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GDS.Core.Events;


namespace GDS.Core {
    /// <summary>
    /// Represents an inventory as a fixed set of named slots. 
    /// Items are assigned to specific slots based on ID, not order or position.
    /// </summary>    
    public class SetBag : Bag {
        public Observable<Dictionary<string, SetSlot>> Data;
        public Dictionary<string, SetSlot> Slots => Data.Value;
    }

    public static class SetBagFactory {
        public static T Create<T>(string id, params string[] slotKeys) where T : SetBag {
            var bag = Activator.CreateInstance<T>();
            var slots = slotKeys.ToDictionary(key => key, key => SlotFactory.Create(key));
            bag.Id = id;
            bag.Data = new(slots);
            return bag;
        }
    }

    public static class SetBagExt {
        public static void Clear(this SetBag bag) {
            string key;
            for (int i = 0; i < bag.Slots.Count; i++) {
                key = bag.Slots.Keys.ElementAt(i);
                bag.Slots[key] = bag.Slots[key] with { Item = Item.NoItem };
            }
        }

        public static Result PlaceItem(this SetBag bag, SetSlot slot, Item item, EventModifiers mods = EventModifiers.None) {
            if (!bag.Accepts(item)) return Result.Fail;
            if (!slot.Accepts(item)) return Result.Fail;

            var hasKey = bag.Slots.ContainsKey(slot.Key);
            if (!hasKey) return Result.Fail;

            UpdateSlot(bag, slot, item);
            return new PlaceItemSuccess(item, slot.Item);
        }

        public static Result PickItem(this SetBag bag, SetSlot slot, EventModifiers mods = EventModifiers.None) {
            UpdateSlot(bag, slot, Item.NoItem);
            return new PickItemSuccess(slot.Item);
        }

        public static Result RemoveItem(this SetBag bag, SetSlot slot) {
            UpdateSlot(bag, slot, Item.NoItem);
            return Result.Success;
        }

        public static void UpdateSlot(this SetBag bag, SetSlot slot, Item item) {
            bag.Slots[slot.Key] = bag.Slots[slot.Key] with { Item = item };
            bag.Data.Notify();
        }

        public static T SetState<T>(this T bag, params SetSlot[] items) where T : SetBag {
            bag.Clear();
            foreach (var (key, item) in items) bag.SetItem(key, item);
            bag.Data.Notify();
            return bag;
        }

        public static bool SetItem(this SetBag bag, string slotKey, Item item) {
            if (!bag.Slots.ContainsKey(slotKey)) {
                Debug.LogWarning("Did not find slot " + slotKey.Orange());
                return false;
            }

            bag.Slots[slotKey] = bag.Slots[slotKey] with { Item = item };
            return true;
        }

    }

}