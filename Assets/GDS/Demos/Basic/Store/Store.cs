using System.Collections.Generic;
using System.Linq;
using GDS.Core.Events;
using GDS.Core;
using GDS.Demos.Basic;
using GDS.Demos.Basic.Events;
using static GDS.Core.LogUtil;

namespace GDS.Demos.Basic
{

    // `Store` is a singleton that contains all the system state, listens to UI events and updates the state. 
    // Only the Store can modify it's internal state.
    public class Store
    {
        public Store()
        {
            LogCreate(this);
            // Listen to reset event - it is triggered when entering play mode
            EventBus.Global.On<Reset>(e => Init());

            Bus.On<PickItem>(e => OnPickItem(e as PickItem));
            Bus.On<PlaceItem>(e => OnPlaceItem(e as PlaceItem));
            Bus.On<SortBag>(e => OnSortBag(e as SortBag));

            Bus.On<CollectAll>(e => OnCollectAll(e as CollectAll));
            Bus.On<HotbarUse>(e => OnHotbarUse(e as HotbarUse));
            Bus.On<DestroyCurrentItem>(e => OnDestroyCurrentItem(e as DestroyCurrentItem));
            Bus.On<DiscardCurrentItem>(e => OnDiscardCurrentItem(e as DiscardCurrentItem));
            Bus.On<SaveCharacterData>(e => OnSaveCharacterDto(e as SaveCharacterData));
            Bus.On<LoadCharacterData>(e => OnLoadCharacterDto(e as LoadCharacterData));

            Bus.On<ToggleInventory>(e => OnToggleInventory(e as ToggleInventory));
            Bus.On<CloseWindow>(e => OnCloseWindow(e as CloseWindow));
            Bus.On<OpenWindow>(e => OnOpenWindow(e as OpenWindow));
            Bus.On<CloseInventory>(e => OnCloseInventory(e as CloseInventory));
            Bus.On<ToggleCharacterSheet>(e => OnToggleCharacterSheet(e as ToggleCharacterSheet));

            Bus.OnAny<Result>(e => LogResultEvent(e));

            CharacterSheet = new CharacterSheet(Equipment);
            // Re-emit the ui changed event on the global bus
            // This event is used in scripts to control certain behaviors
            UiOpen.OnChange += value => EventBus.Global.Publish(new UiStateChanged(value));

            // Create an interactible bags list
            // It is used as a data source in StoreBag editor script
            Bags = new() { Chest, Stash, Shop, CraftingBench };

            // Initialize
            Init();
        }

        // Event bus used to pass messages between UI and the Store
        public static readonly EventBus Bus = new();
        public static readonly Store Instance = new();

        public readonly Main Main = ListBagFactory.Create<Main>("Main", 40);
        public readonly Equipment Equipment = new Equipment("Equipment");
        public readonly ListBag Hotbar = ListBagFactory.Create<ListBag>("Hotbar", 5, Filters.Consumable);

        public readonly Chest Chest = ListBagFactory.Create<Chest>("chest", 10);
        public readonly Shop Shop = ListBagFactory.Create<Shop>("shop", 20);
        public readonly Stash Stash = ListBagFactory.Create<Stash>("stash", 80);
        public readonly CraftingBench CraftingBench = ListBagFactory.Create<CraftingBench>("crafting-bench", 3, Filters.Material);

        public readonly CharacterSheet CharacterSheet;

        public readonly Observable<Item> DraggedItem = new(Item.NoItem);
        public readonly Observable<bool> UiOpen = new(false);
        public readonly Observable<object> SideWindow = new(Bag.NoBag);
        public readonly Observable<int> Gold = new(0);

        // This list is used in editor scripts to allow referencing a Bag directly instead of by Id.
        public readonly List<Bag> Bags;


        // Resets the Store state. 
        // Adds initial items to all Bags.        
        void Init()
        {
            LogInit(this);

            UiOpen.SetValue(false);
            DraggedItem.SetValue(Item.NoItem);
            SideWindow.SetValue(Bag.NoBag);
            Gold.SetValue(10000);

            Equipment.SetState(InitialState.Equip);
            Main.SetState(InitialState.Main);
            Hotbar.SetState(InitialState.Hotbar);
            Shop.SetState(InitialState.Shop);
            Chest.SetState(InitialState.Chest);
            Stash.SetEmptyState();
            CraftingBench.SetEmptyState();
        }

        // PickItem event handler.
        // If picking is successful, sets the new state of Dragged Item and notifies the inventory from which the item was picked.
        // Handles various cases like ctrl+click and shift+click.        
        void OnPickItem(PickItem e)
        {
            LogEvent(e);

            // Handle Shop logic: return early if the player can't afford the item
            if (e.Bag is Shop && BagExt.CantAfford(e.Slot.Item, Gold.Value))
            {
                Bus.Publish(new CantAfford(e.Slot.Item));
                return;
            }

            // Handle Crafting logic: pick the result of a successful craft and return early
            if (e.Bag is CraftingBench && e.Slot is CraftOutcomeSlot)
            {
                var craftResult = BagExt.CraftItem(e, Main, DraggedItem);
                Bus.Publish(craftResult);
                return;
            }

            // Compute the other bag and perform the action of pick or move
            Bag otherBag = BagExt.OtherBag(e.Bag, SideWindow.Value, Main);
            var result = GDS.Core.BagExt.PickOrMoveItem(e, otherBag);
            GDS.Core.BagExt.UpdateDragged(result, DraggedItem);

            // Convert result to a buy or sell event if needed, update gold, and publish the event
            var gameEvent = BagExt.ToGameEvent(result, e.Bag, otherBag);
            BagExt.UpdateGold(gameEvent, Gold);
            Bus.Publish(gameEvent);

            // ℹ️ Optional: This logic could be chained fluently like this:
            // GDS.Core.BagExt.PickOrMoveItem(e, otherBag)
            //     .UpdateDragged(DraggedItem)
            //     .ToGameEvent(e.Bag, otherBag)
            //     .UpdateGold(Gold)
            //     .Publish(Bus);
        }


        // PlaceItem event handler
        // If placing is successful, sets the new state of the Dragged Item and notifies the
        // inventory in which the item was placed
        void OnPlaceItem(PlaceItem e)
        {
            LogEvent(e);

            // Attempt to place the item into the target bag and clear the dragged item
            var result = GDS.Core.BagExt.PlaceItem(e);
            GDS.Core.BagExt.UpdateDragged(result, DraggedItem);

            // Convert result to a sell event if needed, update gold, and publish the event
            var gameEvent = BagExt.ToGameEvent(result, e.Bag, Bag.NoBag);
            BagExt.UpdateGold(gameEvent, Gold);
            Bus.Publish(gameEvent);

            // ℹ️ Optional: This logic could be chained fluently like this:
            // GDS.Core.BagExt.PlaceItem(e)
            //     .UpdateDragged(DraggedItem)
            //     .ToGameEvent(e.Bag, Bag.NoBag)
            //     .UpdateGold(Gold)
            //     .Publish(Bus);
        }


        // Adds an item to the main inventory if possible        
        // Return a Success or BagFull event
        public Result LootItem(Item item)
        {
            LogEvent("LootItem");

            var result = Main.AddItems(item);
            if (result is Success) Bus.Publish(new LootItemSuccess(item));
            else Bus.Publish(result);
            return result;
        }


        // Trigerred when an item is dropped "on the ground".
        void OnDiscardCurrentItem(DiscardCurrentItem e)
        {
            LogEvent(e);
            Bus.Publish(new DiscardItemSuccess(DraggedItem.Value));
            DraggedItem.SetValue(Item.NoItem);
        }


        // Trigerred when an item is destroyed
        void OnDestroyCurrentItem(DestroyCurrentItem e)
        {
            LogEvent(e);
            Bus.Publish(new DestroyItemSuccess(DraggedItem.Value));
            DraggedItem.SetValue(Item.NoItem);
        }


        // Moves all items from a bag to main inventory
        // The action can partially fail (not all items can fit), in which case, the items that did not fit, will remain in the source bag.
        void OnCollectAll(CollectAll e)
        {
            LogEvent(e);
            var items = e.Bag.Slots.Where(slot => slot.IsFull()).Select(slot => slot.Item).ToArray();
            var result = Main.AddItems(items);
            if (result is Success) e.Bag.SetEmptyState();
            if (result is CantFitAll r) e.Bag.SetState(r.Items.ToArray());
            Bus.Publish(result);
        }


        // Consumes an item in the hot-bar.
        // If item quantity is 0 after the action, it becomes a `NoItem`.
        void OnHotbarUse(HotbarUse e)
        {
            LogEvent(e);

            if (Hotbar.Slots[e.Index].IsEmpty()) return;
            Hotbar.Slots[e.Index] = Hotbar.Slots[e.Index] with { Item = Hotbar.Slots[e.Index].Item.Decrement() };
            Hotbar.Data.Notify();

            Bus.Publish(new ConsumeItemSuccess(Hotbar.Slots[e.Index].Item));
            EventBus.Global.Publish(new ConsumeItemSuccess(Hotbar.Slots[e.Index].Item));
        }


        // Toggles main inventory `Open` state
        void OnToggleInventory(ToggleInventory e)
        {
            LogEvent(e);
            UiOpen.SetValue(!UiOpen.Value);

            if (UiOpen.Value == true) return;
            SideWindow.SetValue(Bag.NoBag);
        }


        // Closes main inventory and any other open windows
        void OnCloseInventory(CloseInventory e)
        {
            LogEvent(e);
            UiOpen.SetValue(false);
            SideWindow.SetValue(Bag.NoBag);
        }


        // Opens a 'side' window.
        void OnOpenWindow(OpenWindow e)
        {
            LogEvent(e);

            SideWindow.SetValue(e.Handle);
            UiOpen.SetValue(true);
        }


        // Closes a window. Also closes other windows if current window is the main inventory.
        void OnCloseWindow(CloseWindow e)
        {
            LogEvent(e);
            if (e.Handle == Main) UiOpen.SetValue(false);
            SideWindow.SetValue(Bag.NoBag);
        }


        // Toggles `CharacterSheet` window
        void OnToggleCharacterSheet(ToggleCharacterSheet e)
        {
            LogEvent(e);
            if (SideWindow.Value is CharacterSheet) SideWindow.SetValue(Bag.NoBag);
            else SideWindow.SetValue(CharacterSheet);
            UiOpen.SetValue(true);

        }


        // Saves Player data        
        void OnSaveCharacterDto(SaveCharacterData e)
        {
            LogEvent(e);

            CharacterDto player = new("Jim", 1, Equipment, Main, Hotbar);
            SaveSystem.Save(player);
            Bus.Publish(new SaveSuccess());
        }


        // Loads Player data        
        void OnLoadCharacterDto(LoadCharacterData e)
        {
            LogEvent(e);

            CharacterDto player = SaveSystem.Load();
            if (player == null) return;

            Main.SetState(player.inventory.Select(DtoExt.Create).ToArray());
            Hotbar.SetState(player.hotbar.Select(DtoExt.Create).ToArray());
            Equipment.SetState(player.equipment.Select(DtoExt.Create).ToArray());

            Bus.Publish(new LoadSuccess());
        }

        // Sorts a bag
        void OnSortBag(SortBag e)
        {
            LogEvent(e);
            var result = GDS.Core.BagExt.SortBag(e.Bag, e.SortFn, e.Desc);
            LogEvent(result);
        }
    }
}