using System.Linq;
using UnityEngine.UIElements;
using GDS.Core;
using GDS.Core.Events;
using GDS.Demos.Basic.Views;


namespace GDS.Demos.Basic {

    public class CraftingBenchWindow : VisualElement {

        public CraftingBenchWindow(CraftingBench bag) {
            var Outcome = new BasicSlotView(bag, bag.OutcomeSlot.Value);
            var OutcomeOverlay = Dom.Div("crafting-bench-outcome-slot-overlay");
            var Arrow = Dom.Label("crafting-bench-arrow", "↓");

            this.Add("window crafting-bench gap-v-20",
                Components.CloseButton(bag),
                Dom.Title("Crafting bench"),
                Dom.Div("align-items-center",
                    Dom.Label("Place materials here:"),
                    new BasicListView(bag),
                    Arrow,
                    Dom.Label("Outcome:"),
                    Dom.Div(
                        Outcome.WithClass("crafting-bench-outcome-slot"),
                        OutcomeOverlay
                    )
                ),
                Hints()
            );

            this.Observe(bag.OutcomeSlot, value => Outcome.Data = value);
            this.SubscribeTo<CraftItemSuccess>(Store.Bus, (e) => {
                OutcomeOverlay.TriggerClassAnimation("craft-success", 250);
            });
        }

        VisualElement Hints() {
            var recipes = Recipes.All.Keys.ToArray();
            var el = Dom.Div(
                Dom.Label("Try:"),
                Dom.Div("hints", recipes.Select(r => Dom.Label(Hint(r))).ToArray())
            );
            return el;
        }

        string Hint(Recipe r) => r.Item1?.Name + " - " + r.Item2?.Name + " - " + r.Item3?.Name;
    }
}
