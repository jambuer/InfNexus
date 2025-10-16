using System.Collections.Generic;
using GDS.Core;

namespace GDS.Demos.Basic {
    public record CraftOutcomeSlot(Item Item) : Slot(Item);
    public class CraftingBench : ListBag {
        override public void Init() {
            // Update the Outcome slot on change
            Data.OnChange += value => {
                var recipe = new Recipe(value[0].Item.Base, value[1].Item.Base, value[2].Item.Base);
                var outcome = Recipes.All.GetValueOrDefault(recipe);
                Item item = outcome is null ? Item.NoItem : outcome.Create();
                OutcomeSlot.SetValue(OutcomeSlot.Value with { Item = item });
            };
        }
        public Observable<CraftOutcomeSlot> OutcomeSlot = new(new CraftOutcomeSlot(Item.NoItem) { Accepts = GDS.Core.Filters.Nothing });
    }
}