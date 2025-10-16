using System;
using UnityEngine.UIElements;
using GDS.Core;
using GDS.Core.Views;
using GDS.Core.Events;
using GDS.Demos.Basic.Views;
using GDS.Demos.Basic.Events;
using static GDS.Core.Dom;
namespace GDS.Demos.Basic {

    // Use UxmlElement attribute for Unity 6
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]    
#endif

    public partial class RootLayer : VisualElement {

        // Use UxmlFactory for older Unity versions
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<RootLayer> { }
#endif        

        Store store = Store.Instance;
        EventBus bus = Store.Bus;
        public RootLayer() {

            // Create visual elements
            var Backdrop = Div("backdrop").PickDefault();
            var DropTarget = new DropTargetView(store.DraggedItem, _ => bus.Publish(new DiscardCurrentItem()));
            var SideContainer = Div("flex-1 p-50 align-items-end");
            var MainContainer = Div("flex-1 p-50 align-items-start");
            var Container = Div("window-container");

            // Add elements to root
            this.Add("root-layer column",
                Backdrop,
                DropTarget,
                HelpPanel(),
                Container.Add(
                    SideContainer,
                    MainContainer.Add(
                        new InventoryView().WithClass("mb-20"),
                        DestroyItemButton()
                    )
                ),
                new LogMessageView(bus)
            )


            // Add behaviors
            .WithRestrictedSlotBehavior(store.DraggedItem)
            .WithDragToPickBehavior(store.DraggedItem, bus)
            .WithGhostItemBehavior(store.DraggedItem, new BasicItemView())
            .WithItemTooltipBehavior(new Tooltip())

            // Watch for changes on UIOpen state and show a backdrop
            .Observe(store.UiOpen, value => {
                Container.SetVisible(value);
                Backdrop.SetVisible(value);
            })

            // Watch for changes on SideWindow and create a new window
            .Observe(store.SideWindow, (handle) => {
                SideContainer.Clear();
                SideContainer.Add(CreateWindowView(handle));
            });

            // Ignore picks on the root element
            // This allows hovering and clicking world items
            // This is done AttachToPanel callback because doing it in constructor doesn't work for some reason
            RegisterCallback<AttachToPanelEvent>(_ => pickingMode = PickingMode.Ignore);

        }

        /// <summary>
        ///  Creates a side window 
        /// </summary>
        VisualElement CreateWindowView(object handle) => handle switch {
            CharacterSheet h => new CharacterSheetWindow(h),
            Stash h => new StashWindow(h),
            Shop h => new ShopWindow(h),
            CraftingBench h => new CraftingBenchWindow(h),
            ListBag h => new ChestWindow(h),
            _ => Div()
        };

        /// <summary>
        /// Creates the Help Panel 
        /// </summary>
        VisualElement HelpPanel() {
            var HelpText = Label("description", $"[WASD]: Move {"character".Orange()}\n[C]: Character Sheet\n[Tab] or [I]: Toggle inventory\n[ESC]: Close all windows\n[1-5]: Consume items in Hotbar\nDrag, Click or Ctrl+Click to move items\n[Shift+Click]: Pick half a stack (stackable items)");
            var Buttons = ButtonBar();
            var ToggleButtons = Button("Toggle button bar", () => Buttons.Toggle());
            var ToggleHelpText = Button("Toggle help text", () => HelpText.Toggle());
            var Save = Button("Save", () => bus.Publish(new SaveCharacterData()));
            var Load = Button("Load", () => bus.Publish(new LoadCharacterData()));

            return Div("help-panel gap-v-8",
                Div("buttons-container",
                    Div("row", ToggleButtons, ToggleHelpText, Save, Load),
                    Buttons
                ),
                HelpText
            )
            // Show a green tick mark for a second when save was successful
            .SubscribeTo<SaveSuccess>(Store.Bus, v => {
                Save.text = "Save " + "✓".Green();
                Save.TriggerClassAnimation("", 1000, () => Save.text = "Save");
            })

            // Show a green tick mark for a second when load was successful
            .SubscribeTo<LoadSuccess>(Store.Bus, v => {
                Load.text = "Load " + "✓".Green();
                Load.TriggerClassAnimation("", 1000, () => Load.text = "Load");
            });
        }

        /// <summary>
        /// Creates the button bar. Each button publishes an event. 
        /// </summary>
        VisualElement ButtonBar() =>
            Div("row",
                Button("Shop", () => bus.Publish(new OpenWindow(store.Shop))),
                Button("Chest", () => bus.Publish(new OpenWindow(store.Chest))),
                Button("Crafting", () => bus.Publish(new OpenWindow(store.CraftingBench))),
                Button("Stash", () => bus.Publish(new OpenWindow(store.Stash))),
                Button($"Character [{"C".Orange()}]", () => bus.Publish(new ToggleCharacterSheet())),
                Button("mr-50", $"Inventory [{"Tab".Orange()}]", () => bus.Publish(new ToggleInventory())),
                Button("Reset", () => EventBus.Global.Publish(new Reset()))
            );

        /// <summary>
        /// Creates the `Destroy Item` button. Publishes a `destroy` event when an item is dropped on it. 
        /// </summary>
        VisualElement DestroyItemButton() {
            var btn = Div("action-button");
            Action<Item> action = item => {
                bus.Publish(new DestroyCurrentItem());
                btn.TriggerClassAnimation("on-drop-effect");
            };
            btn.Add(
                Div("destroy-item-button"),
                new DropTargetView(store.DraggedItem, action)
            );
            return btn;
        }
    }
}
