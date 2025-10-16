using GDS.Core;
using GDS.Core.Events;
using UnityEngine.UIElements;
using static GDS.Core.Dom;

namespace GDS.Demos.Basic.Views {

    public class SortByComponent : VisualElement {
        public SortByComponent(Bag bag, EventBus bus) {
            var desc = false;
            var lastField = "name";
            this.Add("row align-items-center",
                Label("Sort by:"),
                Button("Name", () => DoSortBy("name", i => i.Name)),
                Button("Rarity", () => DoSortBy("rarity", i => i.Rarity()))
            );

            void DoSortBy(string field, SortFn sortFn) {
                desc = lastField == field ? !desc : false;
                lastField = field;
                bus.Publish(new SortBag(bag, sortFn, desc));
            }
        }

    }
}