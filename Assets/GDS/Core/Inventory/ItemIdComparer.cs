
using System.Collections.Generic;

namespace GDS.Core {
    public class ItemIdComparer : IEqualityComparer<Item> {
        public bool Equals(Item x, Item y) {
            if (x is null || y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Item item) => item.Id;

    }

}