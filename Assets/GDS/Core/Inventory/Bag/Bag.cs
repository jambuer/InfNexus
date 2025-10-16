using System;
using GDS.Core.Events;
using UnityEngine;

namespace GDS.Core {
    public abstract class Bag {
        public static NoBag NoBag = new();
        public string Id = "NoBag";
        public Predicate<Item> Accepts = (_) => true;
        public bool AllowStacking = true;
        public override string ToString() => $"{Id} ({GetType().Name})";
    };
    public class NoBag : Bag { }

    public static class BagExt {

        /// <summary>
        /// Picks an item from the source bag, or moves it to the target bag if Ctrl is held.
        /// Returns a failed result if moving but no target bag is provided.
        /// </summary>
        /// <returns>A <see cref="Result"/> representing the outcome of the pick or move operation.
        /// It can be either a Fail, a PickItemSuccess or a MoveItemSuccess.
        /// </returns>
        public static Result PickOrMoveItem(PickItem e, Bag toBag) => (e.Mods.Ctrl(), toBag) switch {
            (true, NoBag) => Result.Fail,
            (true, _) => MoveItem(e, toBag),
            _ => PickItem(e)
        };

        /// <summary>
        /// Picks an Item from a Bag.
        /// </summary>
        public static Result PickItem(PickItem e) => PickItem(e.Bag, e.Slot, e.Mods);
        public static Result PickItem(this Bag bag, Slot slot, EventModifiers mods = EventModifiers.None) => (bag, slot) switch {
            (DenseListBag b, ListSlot s) => DenseListBagExt.PickItem(b, s, mods),
            (ListBag b, ListSlot s) => ListBagExt.PickItem(b, s, mods),
            (SetBag b, SetSlot s) => SetBagExt.PickItem(b, s, mods),
            _ => Result.Fail
        };

        /// <summary>
        /// Places an Item in a Bag.
        /// </summary>
        public static Result PlaceItem(PlaceItem e) => PlaceItem(e.Bag, e.Slot, e.Item, e.Mods);
        public static Result PlaceItem(BagItem bi) => PlaceItem(bi.Bag, bi.Slot, bi.Item);
        public static Result PlaceItem(this Bag bag, Slot slot, Item item, EventModifiers mods = EventModifiers.None) => (bag, slot) switch {
            (DenseListBag b, ListSlot s) => DenseListBagExt.PlaceItem(b, s, item, mods),
            (ListBag b, ListSlot s) => ListBagExt.PlaceItem(b, s, item, mods),
            (SetBag b, SetSlot s) => SetBagExt.PlaceItem(b, s, item, mods),
            _ => Result.Fail
        };

        /// <summary>
        /// Adds Items to a Bag.
        /// </summary>
        public static Result AddItems(this Bag bag, params Item[] items) => bag switch {
            ListBag b => b.AddItems(items),
            _ => Result.Fail
        };

        /// <summary>
        /// Removes an Item from a Bag.
        /// </summary>
        public static Result RemoveItem(this Bag bag, Slot slot) => (bag, slot) switch {
            (SetBag b, SetSlot s) => b.RemoveItem(s),
            (ListBag b, ListSlot s) => b.RemoveItem(s),
            _ => Result.Fail
        };

        /// <summary>
        /// Moves Items from a Bag to another target Bag.
        /// </summary>
        public static Result MoveItem(PickItem e, Bag toBag) => MoveItem(e.Bag, e.Slot, toBag);
        public static Result MoveItem(this Bag bag, Slot slot, Bag toBag) => (bag, slot, toBag) switch {
            (DenseListBag b, ListSlot s, ListBag toB) => MoveItem(b, s, toB),
            (ListBag b, ListSlot s, ListBag toB) => MoveItem(b, s, toB),
            (SetBag b, SetSlot s, ListBag toB) => MoveItem(b, s, toB),
            _ => Result.Fail.LogAndReturn("Could not move item - no expression matched")
        };

        /// <summary>
        /// Moves an Item from a DenseList to a List.
        /// </summary>
        static Result MoveItem(DenseListBag bag, ListSlot slot, ListBag toBag) {
            if (!toBag.Accepts(slot.Item)) return new Restricted<Bag>(toBag);
            if (toBag.IsFull()) return new BagFull(toBag);

            bag.RemoveItem(slot);
            toBag.AddItems(slot.Item);
            return new MoveItemSuccess(slot.Item);
        }

        /// <summary>
        /// Moves an Item from a List to another List.
        /// </summary>
        static Result MoveItem<T>(T bag, ListSlot slot, ListBag toBag) where T : ListBag {
            if (!toBag.Accepts(slot.Item)) return new Restricted<Bag>(toBag);
            if (toBag.IsFull()) return new BagFull(toBag);

            bag.RemoveItem(slot);
            toBag.AddItems(slot.Item);
            return new MoveItemSuccess(slot.Item);
        }

        /// <summary>
        /// Moves an Item from a SetBag to ListBag.
        /// </summary>
        static Result MoveItem(SetBag bag, SetSlot slot, ListBag toBag) {
            if (!toBag.Accepts(slot.Item)) return new Restricted<Bag>(toBag);
            if (toBag.IsFull()) return new BagFull(toBag);

            bag.RemoveItem(slot);
            toBag.AddItems(slot.Item);
            return new MoveItemSuccess(slot.Item);
        }



        // TODO: Move this to a more UI-focused utility class
        /// <summary>
        /// Updates the dragged item based on the result of a pick or place operation,
        /// then returns the same result to allow fluent chaining.
        /// </summary>
        public static Result UpdateDragged(this Result result, Observable<Item> dragged) {
            if (result is PickItemSuccess r) dragged.SetValue(r.Item);
            if (result is PlaceItemSuccess p) dragged.SetValue(p.Replaced);
            return result;
        }

        /// <summary>
        /// Publishes the Result on the specified EventBus and returns the same Result, enabling fluent chaining.
        /// </summary>
        public static Result Publish(this Result result, EventBus bus) {
            bus.Publish(result);
            return result;
        }

        public static Result SortBag(this Bag bag, SortFn sortFn, bool desc) {
            sortFn ??= item => item.Name;
            return bag switch {
                ListBag b => ListBagExt.SortBag(b, sortFn, desc),
                _ => new Fail()
            };
        }
    }
}