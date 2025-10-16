using GDS.Core;
using GDS.Core.Events;
using UnityEngine.UIElements;

namespace GDS.Demos.Basic.Views {
    public static class Components {
        public static Button CloseButton(object handle) => Dom.Button("close-button", "", () => Store.Bus.Publish(new CloseWindow(handle)));
        public static SortByComponent SortBy(Bag bag, EventBus bus) => new SortByComponent(bag, bus);
    }
}