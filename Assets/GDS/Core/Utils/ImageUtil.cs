using UnityEngine;

namespace GDS.Core {
    public static class ImageUtil {
        // Why is this not in Inventory/Item
        // -- To keep Item free of dependencies (UnityEngine)
        public static Sprite Image(this Item item) => Resources.Load<Sprite>(item.Base.Icon);
    }
}