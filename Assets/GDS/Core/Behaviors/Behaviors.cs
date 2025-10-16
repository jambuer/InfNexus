using System;
using UnityEngine;
using UnityEngine.UIElements;
using GDS.Core.Events;
using GDS.Core.Views;
using UnityEngine.InputSystem;

namespace GDS.Core {
    public static class Behaviors {

        /// <summary>
        /// Behavior responsible for picking items. Doesn't have any logic itself, instead sends a message on a channel.
        /// An item can be picked by either clicking or dragging. Minimum drag distance is 32px by default.
        /// </summary>        
        public static VisualElement WithDragToPickBehavior(this VisualElement element, Observable<Item> draggedItem, EventBus bus, bool doubleClickEnabled = false, int MinDragDistance = 32) {
            var dragStarted = false;
            var mouseDownPos = new Vector2(-1, -1);
            var target = new VisualElement();

            // This flag controls the validity of mouse event - it prevents from accidentally picking an item when opening an inventory after interacting with the scene
            var validEvent = true;

            void OnMouseDown(PointerDownEvent e) {
                if (e.button != 0) return;
                if (draggedItem.Value is not NoItem) return;
                if (e.target == element) { validEvent = false; return; }
                mouseDownPos = e.position;
                target = e.target as VisualElement;
                dragStarted = true;
                validEvent = true;
            }

            void OnMouseMove(PointerMoveEvent e) {
                if (validEvent == false) return;
                if (dragStarted == false) return;
                if (draggedItem.Value is not NoItem) return;
                if (Math.Abs(mouseDownPos.x - e.position.x) < MinDragDistance && Math.Abs(mouseDownPos.y - e.position.y) < MinDragDistance) return;

                CustomEvent evt = target switch {
                    ISlotView t when draggedItem.Value is NoItem && t.Slot.IsFull() => new PickItem(t.Bag, t.Slot, e.modifiers),
                    _ => new NoEvent()
                };

                dragStarted = false;

                bus.Publish(evt);
            }


            void OnMouseUp(PointerUpEvent e) {
                if (validEvent == false) { return; }
                if (e.button != 0) return;
                dragStarted = false;

                var canPick = doubleClickEnabled == false || e.modifiers != EventModifiers.None;

                CustomEvent evt = e.target switch {
                    ISlotView t when canPick && draggedItem.Value is NoItem && t.Slot.IsFull() /*&& target == t*/ => new PickItem(t.Bag, t.Slot, e.modifiers),
                    ISlotView t when doubleClickEnabled && e.clickCount % 2 == 0 && draggedItem.Value is NoItem && t.Slot.IsFull() /*&& target == t*/ => new DoubleClickItem(t.Bag, t.Slot, e.modifiers),
                    ISlotView t when draggedItem.Value is not NoItem => new PlaceItem(t.Bag, t.Slot, draggedItem.Value, e.modifiers),

                    _ => new NoEvent()
                };
                bus.Publish(evt);
            }



            element.RegisterCallback<PointerDownEvent>(OnMouseDown);
            element.RegisterCallback<PointerUpEvent>(OnMouseUp);
            element.RegisterCallback<PointerMoveEvent>(OnMouseMove);
            return element;
        }

        /// <summary>
        /// "Disables" a slot by applying a USS class when conditions are met (a bag or a slot does not accept the item).
        /// </summary>
        public static VisualElement WithRestrictedSlotBehavior(this VisualElement element, Observable<Item> draggedItem) {
            var NoDiv = Dom.Div();
            var lastHoveredItem = NoDiv;

            EventCallback<PointerOverEvent> OnMouseOver = (e) => {
                if (e.target is not ISlotView view) return;
                lastHoveredItem = e.target as VisualElement;
                if (draggedItem.Value is NoItem) return;
                bool legalAction = view.Bag.Accepts(draggedItem.Value) && view.Slot.Accepts(draggedItem.Value);

                string className = legalAction ? "legal-action" : "illegal-action";
                lastHoveredItem.WithClass(className);
            };

            EventCallback<PointerOutEvent> OnMouseOut = (e) => {
                if (draggedItem.Value is NoItem) return;
                if (e.target is not ISlotView) return;
                if (lastHoveredItem == null) return;
                lastHoveredItem.WithoutClass("illegal-action legal-action");
                lastHoveredItem = NoDiv;
            };

            element.Observe(draggedItem, item => {
                if (item is NoItem) lastHoveredItem.WithoutClass("illegal-action legal-action");
            });

            element.RegisterCallback(OnMouseOver);
            element.RegisterCallback(OnMouseOut);

            return element;
        }

        /// <summary>
        /// Shows the dragged item at cursor position as it is being dragged. 
        /// Should be acontainerached to the root element (dragged element moves in screen space).
        /// Requires `ghost-item` uss class (absolute pos, % translate).
        /// </summary>
        public static VisualElement WithGhostItemBehavior(this VisualElement root, Observable<Item> draggedItem, Core.Views.Component<Item> itemView) {

            var ghost = Dom.Div("ghost-item", itemView);
            ghost.PickIgnoreAll();
            root.Add(ghost);

            EventCallback<PointerMoveEvent> OnPointerMove = e => {
                ghost.style.left = e.localPosition.x;
                ghost.style.top = e.localPosition.y;
            };

            DomExt.Observe(root, draggedItem, item => {
                if (item is NoItem) { ghost.Hide(); return; }

                itemView.Data = item;
                ghost.Show();
            });

            root.RegisterCallback(OnPointerMove);
            return root;
        }

        /// <summary>
        /// Adds a layer that will serve as a drop target. Requires USS classes.
        /// </summary>        
        public static T WithDropTargetBehavior<T>(this T element, Observable<Item> dragged, Action<Item> callback) where T : VisualElement {
            element.Add(new DropTargetView(dragged, callback));
            return element;
        }

        /// <summary>
        /// Shows an item tooltip 
        /// </summary>
        public static VisualElement WithItemTooltipBehavior(this VisualElement root, Component<BagItem> tooltip) => WithItemTooltipBehavior(root, tooltip, DefaultPosition, () => { });
        public static VisualElement WithItemTooltipBehavior(this VisualElement root, Component<BagItem> tooltip, Func<Rect, Rect, Rect, (float, float)> positionStrategy, Action callback) {

            var pollingInterval = 50;
            var container = tooltip.PickIgnoreAll();
            var visible = false;
            container.Hide();
            root.Add(container);
            Item lastItem = Item.NoItem;
            Rect lastItemBounds = new();

            void ConditionalHide() { /*return;*/ if (visible == true) { visible = false; lastItem = Item.NoItem; container.Hide(); } }
            void ConditionalShow() { if (visible == false) { visible = true; container.Show(); } }

            Rect rootBounds = root.worldBound;
            Vector2 screenPos;
            VisualElement picked;
            VisualElement lastPicked = new();
            float arw;
            float arh;
            bool skipFlag = true;

            tooltip.RegisterCallback<GeometryChangedEvent>(e => {
                if (container.worldBound.width == 0) return;
                if (skipFlag) return;

                PositionTooltip();
                skipFlag = true;
            });

            void PositionTooltip() {
                var (left, top) = positionStrategy(lastItemBounds, container.worldBound, rootBounds);
                container.style.left = left;
                container.style.top = top;
            }

            root.schedule.Execute(() => {
                arw = root.worldBound.width / Screen.width;
                arh = root.worldBound.height / Screen.height;
                screenPos = Mouse.current.position.ReadValue();
                screenPos.x *= arw;
                screenPos.y = (Screen.height - screenPos.y) * arh;
                picked = root.panel.Pick(screenPos);

                // TODO: optimize this
                (Bag bag, Slot slot, Item item, Rect bounds) = picked switch {
                    ISlotView s => (s.Bag, s.Slot, s.Slot.Item, picked.worldBound),
                    _ => (Bag.NoBag, new Slot(Item.NoItem), Item.NoItem, new Rect())
                };

                if (item is NoItem) { ConditionalHide(); return; }
                if (item.Id == lastItem.Id) return;
                lastItem = item;
                lastItemBounds = bounds;

                tooltip.Data = new(bag, slot, item);
                PositionTooltip();
                skipFlag = false;

                ConditionalShow();

                callback();

            }).Every(pollingInterval);

            return root;
        }

        /// <summary>
        /// <para>Try to position the tooltip next to the element, first on Top, then Left, then Right.</para>
        /// <para>Will stick </para>
        /// </summary>
        /// <returns>The new top-left</returns>
        public static (float left, float top) DefaultPosition(Rect itemBounds, Rect tooltipBounds, Rect rootBounds) {
            float left, top;
            top = itemBounds.yMin - tooltipBounds.height;
            left = itemBounds.center.x - tooltipBounds.width / 2;

            // TODO: turn into switch statement
            if (top < 0) {
                top = 0;
                left = itemBounds.xMin - tooltipBounds.width;
                if (left < 0) {
                    left = itemBounds.xMax;
                }
            } else {
                if (left < 0) {
                    left = 0;
                }
                if (left + tooltipBounds.width > rootBounds.width) {
                    left = rootBounds.width - tooltipBounds.width;
                }
            }

            return (left, top);
        }


    }


}