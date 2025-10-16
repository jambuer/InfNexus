using UnityEngine.UIElements;
using GDS.Core.Events;
using System.Linq;

namespace GDS.Core.Views {

    /// <summary>
    /// Displays a message queue in the bottom left corner
    /// Removes message after a delay (3 seconds)
    /// Hides itself when queue is empty
    /// </summary>
    public class LogMessageView : VisualElement {

        public LogMessageView(EventBus bus) {
            this.WithClass("message-container").Hide();

            this.SubscribeTo<CraftItemSuccess>(bus, e => AddMessage("Crafted ".Green() + Message(e)));
            this.SubscribeTo<DestroyItemSuccess>(bus, e => AddMessage("Destroyed ".Red() + Message(e)));
            this.SubscribeTo<ConsumeItemSuccess>(bus, e => AddMessage("Consumed ".Green() + (e as ConsumeItemSuccess).Item.Name));
            this.SubscribeTo<BuyItemSuccess>(bus, e => AddMessage("Bought ".Blue() + Message(e)));
            this.SubscribeTo<SellItemSuccess>(bus, e => AddMessage("Sold ".Blue() + Message(e)));
            this.SubscribeTo<CantAfford>(bus, e => AddMessage("Not enough gold to buy ".Red() + Message(e)));
            this.SubscribeTo<SaveSuccess>(bus, e => AddMessage("Save success!".Green()));
            this.SubscribeTo<LoadSuccess>(bus, e => AddMessage("Load success!".Green()));

            this.SubscribeTo<BagFull>(bus, e => AddMessage("Bag full! ".Yellow()));
            this.SubscribeTo<Restricted<Bag>>(bus, e => AddMessage("Bag does not accept item! ".Yellow()));
            this.SubscribeTo<Restricted<Slot>>(bus, e => AddMessage("Slot does not accept item! ".Yellow()));
            this.SubscribeTo<CantFitAll>(bus, e => AddMessage($"Bag full! Cannot fit {(e as CantFitAll).Items.Count()} items. ".Yellow()));
        }

        string Message(CustomEvent e) => e switch {
            ItemSuccess s => ItemText(s.Item),
            ItemFail s => ItemText(s.Item),
            _ => ""
        };
        string ItemText(Item item) => item.Stackable ? $"{item.Name} ({item.Quant})" : $"{item.Name}";



        void AddMessage(string message) {
            this.Show();
            var label = Dom.Label("message", message);
            Add(label);
            schedule.Execute(() => {
                Remove(label);
                if (childCount == 0) this.Hide();
            }).ExecuteLater(3000);
        }
    }
}
