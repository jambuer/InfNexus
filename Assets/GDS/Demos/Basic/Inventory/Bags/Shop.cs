using System.Linq;
using GDS.Core;

namespace GDS.Demos.Basic {
    public class Shop : DenseListBag {
        public bool Infinite = false;
        public void Reroll() {
            var items = Enumerable.Range(0, Size).Select(_ => ItemFactory.CreateRandom());
            this.SetState(items.ToArray());
        }
    }
}