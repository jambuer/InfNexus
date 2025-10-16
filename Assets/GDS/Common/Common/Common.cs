using GDS.Core;
using UnityEngine;

namespace GDS.Common {
    public interface IHighlight {
        void Highlight();
        void Unhighlight();
    }

    public interface IWorldItem {
        Item Item { get; set; }
        GameObject GameObject { get; }
    }
}