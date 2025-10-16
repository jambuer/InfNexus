using System;
using UnityEngine;
using GDS.Core.Events;
using GDS.Demos.Basic.Events;

namespace GDS.Demos.Basic {
    // Publishes events on key press
    // Events are listened by the Store
    // Actions are declared in the `Actions` Input action asset
    public class InventoryInput : MonoBehaviour {

        Action<CustomEvent> Publish = Store.Bus.Publish;

        public void OnToggleCharacterSheet() => Publish(new ToggleCharacterSheet());
        public void OnToggleInventory() => Publish(new ToggleInventory());
        public void OnCloseInventory() => Publish(new CloseInventory());
        public void OnHotbarUse1() => Publish(new HotbarUse(0));
        public void OnHotbarUse2() => Publish(new HotbarUse(1));
        public void OnHotbarUse3() => Publish(new HotbarUse(2));
        public void OnHotbarUse4() => Publish(new HotbarUse(3));
        public void OnHotbarUse5() => Publish(new HotbarUse(4));

    }
}