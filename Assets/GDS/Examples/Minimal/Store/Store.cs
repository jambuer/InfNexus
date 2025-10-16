using GDS.Core.Events;
using GDS.Core;
using static GDS.Core.LogUtil;

namespace GDS.Examples.Minimal {

    /// <summary>
    /// `Store` is a singleton that serves as the central source of truth for system state. It subscribes to UI events and applies state changes in response.
    /// </summary>
    public class Store {
        public Store() {
            LogCreate(this);
            // Listen for Reset event. It is published by an editor script (`PlayModeStateChanged.cs`) when entering play mode. 
            EventBus.Global.On<Reset>(e => Init());
            // Listen to pick and place UI events
            Bus.On<PickItem>(e => OnPickItem(e as PickItem));
            Bus.On<PlaceItem>(e => OnPlaceItem(e as PlaceItem));
            // Initilize 
            Init();
        }

        // Store singleton instance
        public static readonly Store Instance = new();
        // A channel used to pass the events
        public readonly EventBus Bus = new();
        // Main inventory state
        public readonly ListBag Main = ListBagFactory.Create<ListBag>("Main", 20);
        // DraggedItem contains info about the item being dragged (not a reference)
        // Used in Views and Behaviors
        public readonly Observable<Item> DraggedItem = new(Item.NoItem);

        // Define item bases
        ItemBase Apple = new() { Name = "Apple", Icon = "Shared/Images/items/apple" };
        ItemBase Wood = new() { Name = "Wood", Icon = "Shared/Images/items/wood" };
        ItemBase Helmet = new() { Name = "Helmet", Icon = "Shared/Images/items/helmet" };
        ItemBase Armor = new() { Name = "Armor", Icon = "Shared/Images/items/armor" };

        /// <summary>
        /// Resets the Store state
        /// Sets the inventory state by creating items and adding them to the list
        /// </summary>
        void Init() {
            Main.SetState(
                Apple.Create(),
                Wood.Create(),
                Helmet.Create(),
                Armor.Create()
            );
        }

        /// <summary>
        /// PickItem event handler
        /// If picking is successful, updates the inventory and dragged item
        /// </summary>
        void OnPickItem(PickItem e) {
            LogEvent(e);
            e.Bag.PickItem(e.Slot).UpdateDragged(DraggedItem);
        }

        /// <summary>
        /// PlaceItem event handler
        /// If placing is successful, updates the inventory and dragged item
        /// </summary>
        void OnPlaceItem(PlaceItem e) {
            LogEvent(e);
            e.Bag.PlaceItem(e.Slot, e.Item).UpdateDragged(DraggedItem);
        }
    }
}