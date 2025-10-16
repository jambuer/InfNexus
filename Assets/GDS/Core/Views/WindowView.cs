using GDS.Core.Events;
using UnityEngine.UIElements;

namespace GDS.Core.Views {
    public class WindowView : VisualElement {
        public WindowView(object handle, EventBus bus) {
            this.Add("window", Dom.Button("close-button", "", () => bus.Publish(new CloseWindow(handle))));
        }
    }
}