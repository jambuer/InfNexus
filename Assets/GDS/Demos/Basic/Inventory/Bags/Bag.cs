using GDS.Core;
using GDS.Core.Events;

namespace GDS.Demos.Basic {
    public class Main : ListBag { }
    public class Stash : ListBag { }
    public class Chest : ListBag { public Chest() { Accepts = GDS.Core.Filters.Nothing; } }

    public static class BagExt {
        /// <summary>
        /// Determines the "other" bag based on the given event bag, a side bag object, and the main bag.
        /// <para> Used to decide whether an operation is a Pick or a Move. </para>
        /// </summary>
        /// <returns> The "side" Bag, the "main" Bag or NoBag </returns>
        public static Bag OtherBag(Bag eventBag, object sideBag, Bag mainBag) => (eventBag, sideBag) switch {
            (Main, Bag b1) => b1,
            (Bag b1, Bag b2) when b1 == b2 => mainBag,
            _ => Bag.NoBag
        };

        /// <summary>
        /// Creates a new Item and moves it to target Bag if Ctrl is held or sets it as the new value of current DraggedItem.
        /// </summary>        
        /// <returns> The Result of the craft - either a Fail, a BagFull or CraftItemSuccess </returns>
        public static Result CraftItem(PickItem e, ListBag toBag, Observable<Item> dragged) {
            if (!toBag.Accepts(e.Slot.Item)) return Result.Fail;
            if (toBag.IsFull()) return new BagFull(toBag);

            // Create a new item with random rarity
            var newItem = ItemFactory.Create(e.Slot.Item.Base as ItemBase, CommonExt.RandomRarity());
            // Move to inventory
            if (e.Mods.Ctrl()) toBag.AddItems(newItem);
            // or set dragged
            else dragged.SetValue(newItem);

            var bag = e.Bag as ListBag;
            for (var i = 0; i < bag.Size; i++) bag.Slots[i] = bag.Slots[i] with { Item = bag.Slots[i].Item.Decrement() };
            bag.Data.Notify();

            return new CraftItemSuccess(newItem);

        }

        /// <summary>
        /// Transforms a Pick/Place/MoveItemSuccess events into a Buy/SellItemSuccess event if traget Bag is a Shop.
        /// </summary>
        /// <returns> A Buy/SellItemSuccess or the same result to allow fluent chaining. </returns>
        public static Result ToGameEvent(this Result result, Bag bag, Bag otherBag) => (result, bag, otherBag) switch {
            (PickItemSuccess e, Shop, _) => new BuyItemSuccess(e.Item),
            (PlaceItemSuccess e, Shop, _) => new SellItemSuccess(e.Item),
            (MoveItemSuccess e, Shop, _) => new BuyItemSuccess(e.Item),
            (MoveItemSuccess e, _, Shop) => new SellItemSuccess(e.Item),
            _ => result
        };

        /// <summary>
        /// Updates the Gold observable based on the Result type, adding item value if it's a sell operation, and subtracting if it's a buy operation.
        /// </summary>
        /// <returns> The same result to allow fluent chaining. </returns>
        public static Result UpdateGold(this Result result, Observable<int> gold) {
            if (result is SellItemSuccess ev0) gold.SetValue(gold.Value + ev0.Item.SellValue());
            if (result is BuyItemSuccess ev1) gold.SetValue(gold.Value - ev1.Item.Cost());
            return result;
        }

        /// <summary>
        /// Checks if player gold is not enough to purchase the Item
        /// </summary>
        public static bool CantAfford(this Item item, int gold) => item.Cost() > gold;

    }
}