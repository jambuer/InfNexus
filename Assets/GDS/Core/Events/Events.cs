using System.Collections.Generic;
using UnityEngine;

namespace GDS.Core.Events {
    public abstract record CustomEvent() { public static NoEvent NoEvent = new(); }
    public record NoEvent() : CustomEvent;
    public record Reset() : CustomEvent;

    public record Command : CustomEvent;

    public record PickItem(Bag Bag, Slot Slot, EventModifiers Mods) : Command;
    public record PlaceItem(Bag Bag, Slot Slot, Item Item, EventModifiers Mods) : Command;
    public record AddItem(Bag Bag, Item Item) : Command;
    public record DoubleClickItem(Bag Bag, Slot Slot, EventModifiers Mods) : Command;

    public record SortBag(Bag Bag, SortFn SortFn = null, bool Desc = false) : Command;

    public record DestroyCurrentItem : Command;
    public record DiscardCurrentItem : Command;
    public record LootItem(Item Item) : Command;
    public record DiscardItem(Bag Bag, Slot Slot, Item Item) : Command;
    public record DiscardItem2(Item Item) : Command;

    public record UiStateChanged(bool Open) : CustomEvent;
    public record ToggleInventory() : Command;
    public record CloseInventory() : Command;

    public record ToggleWindow(object Handle) : Command;
    public record OpenWindow(object Handle) : Command;
    public record CloseWindow(object Handle) : Command;

    // Character
    public record SaveCharacterData() : Command;
    public record LoadCharacterData() : Command;

    // Grid item
    public record RotateItemEvent() : Command;

    // Result
    public record Result : CustomEvent { public static Fail Fail = new Fail(); public static Success Success = new Success(); };
    public record Success : Result;
    public record Fail : Result;
    public record Success<T>(T Value) : Success;

    // Success
    public record ItemSuccess(Item Item) : Success;
    public record PickItemSuccess(Item Item) : ItemSuccess(Item);
    public record UnstackItemSuccess(Item Item, Item Remaining) : PickItemSuccess(Item);
    public record PlaceItemSuccess(Item Item, Item Replaced) : ItemSuccess(Item);
    public record StackItemSuccess(Item Item, Item Replaced) : PlaceItemSuccess(Item, Replaced);
    public record MoveItemSuccess(Item Item) : ItemSuccess(Item);
    public record DestroyItemSuccess(Item Item) : ItemSuccess(Item);

    public record CraftItemSuccess(Item Item) : ItemSuccess(Item);
    public record BuyItemSuccess(Item Item) : ItemSuccess(Item);
    public record SellItemSuccess(Item Item) : ItemSuccess(Item);
    public record ConsumeItemSuccess(Item Item) : ItemSuccess(Item);
    public record DiscardItemSuccess(Item Item) : ItemSuccess(Item);
    public record LootItemSuccess(Item Item) : ItemSuccess(Item);
    public record SaveSuccess : Success;
    public record LoadSuccess : Success;



    // Fail
    public record Restricted<T>(T Value) : Fail;
    public record BagFull(Bag Bag) : Fail;
    public record CantFitAll(Bag Bag, IEnumerable<Item> Items) : BagFull(Bag);
    public record ItemFail(Item Item) : Fail;
    public record CantAfford(Item Item) : ItemFail(Item);
}