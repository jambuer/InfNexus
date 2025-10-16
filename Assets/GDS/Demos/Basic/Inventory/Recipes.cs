using System.Collections.Generic;
using GDS.Core;


namespace GDS.Demos.Basic {

    public record Recipe(IItemBase Item1, IItemBase Item2, IItemBase Item3);

    public static class Recipes {
        public static readonly Dictionary<Recipe, ItemBase> All = new() {
            {new (Bases.Wood, Bases.Wood, Bases.Steel), Bases.Axe},
            {new (Bases.Wood, Bases.Steel, Bases.Steel), Bases.LongSword},
            {new (Bases.Gem, Bases.Wood, Bases.Steel), Bases.ShortSword},
            {new (Bases.Gem, Bases.Steel, ItemBase.NoItemBase), Bases.BlueAmulet},
        };

    }
}