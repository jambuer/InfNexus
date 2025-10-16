using System;
using System.Linq;
using UnityEngine.UIElements;
using static GDS.Core.Dom;
namespace GDS.Core {
    public static class DebugUtil {
        public static Func<string, string> ColorFn(Slot slot) => slot.IsEmpty() ? ColorUtil.Gray : x => x;
        public static Func<string, string> ColorFn(Item item) => item is NoItem ? ColorUtil.Gray : x => x;
        public static string SlotText(ListSlot slot) => $"{slot.Index}: {ItemText(slot.Item)}";
        public static string SetSlotText(SetSlot slot) => $"[{slot.Key}]: {ItemText(slot.Item)}";
        public static string ItemQuantText(Item item) => item.Stackable ? $"({item.Quant})" : "";
        public static string ItemText(Item item) => $"{item.Name}{ItemQuantText(item)} [id: {item.Id}]";

        public static VisualElement PlayerGoldDebug(Observable<int> gold) {
            var el = Div();
            return el.Observe(gold, value => {
                el.Clear();
                el.Add(
                    Title("Player gold"),
                    Label(value.ToString())
                );
            });
        }


        public static VisualElement DraggedItemDebug(Observable<Item> dragged) => DraggedItemDebug(dragged, ItemText);
        public static VisualElement DraggedItemDebug(Observable<Item> dragged, Func<Item, string> ItemTextFn) {
            var el = Div();
            return el.Observe(dragged, item => {
                el.Clear();
                el.Add(
                    Title("Dragged item"),
                    Label(ColorFn(item)(ItemTextFn(item)))
                );
            });
        }

        public static VisualElement ListBagDebug(ListBag bag) => ListBagDebug(bag, SlotText);
        public static VisualElement ListBagDebug(ListBag bag, Func<ListSlot, string> SlotTextFn) {
            var el = Div();
            return el.Observe(bag.Data, (_) => {
                el.Clear();
                el.Add(
                    Title(bag.Id),
                    Div(bag.Slots
                        .Select(slot => ColorFn(slot)(SlotTextFn(slot)))
                        .Select(Label)
                        .ToArray())
                );
            });
        }

        public static VisualElement SetBagDebug(SetBag bag) => SetBagDebug(bag, SetSlotText);
        public static VisualElement SetBagDebug(SetBag bag, Func<SetSlot, string> SetSlotText) {
            var el = Div();
            return el.Observe(bag.Data, (_) => {
                el.Clear();
                el.Add(
                    Title(bag.Id),
                    Div(bag.Slots.Values
                        .Select(slot => ColorFn(slot)(SetSlotText(slot)))
                        .Select(Label)
                        .ToArray())
                );
            });
        }

        public static VisualElement SideWindowDebug(Observable<object> sideWindow, Observable<Item> draggedItem) {
            var el = Dom.Div();
            var title = Title("Side Window");
            var content = Dom.Div();
            el.Add(title, content);
            el.Observe(sideWindow, value => {
                content.Clear();
                if (value is ListBag b1) { title.text = $"Side Window ({b1.Id})"; content.Add(ListBagDebug(b1)); return; }
                if (value is Bag b) { title.text = $"Side Window ({b.Id})"; return; }
                title.text = "Side Window (not a Bag)";
            });
            return el;
        }



    }
}