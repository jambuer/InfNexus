using GDS.Common;
using GDS.Common.Events;
using GDS.Common.Scripts;
using GDS.Core.Events;
using UnityEngine;

namespace GDS.Demos.Basic {
    public class NearbyAction : MonoBehaviour {
        Store store = Store.Instance;
        string nearbyBagId;
        WorldItem nearbyItem;
        private void Update() {
            if (Input.GetKeyDown(KeyCode.E)) {
                if (nearbyBagId != null) Store.Bus.Publish(new OpenWindow(Store.Instance.Bags.Find(b => b.Id == nearbyBagId)));
                if (nearbyItem != null) EventBus.Global.Publish(new LootWorldItem(nearbyItem));
            }
        }

        void OnTriggerEnter(Collider other) {
            var interactable = other.GetComponent<IHighlight>();
            if (interactable == null) return;
            interactable.Highlight();

            other.TryGetComponent(out StoreBag bag);
            if (bag != null) nearbyBagId = bag.BagId;
            other.TryGetComponent(out WorldItem item);
            if (item != null) nearbyItem = item;
        }

        void OnTriggerExit(Collider other) {
            var interactable = other.GetComponent<IHighlight>();
            if (interactable == null) return;
            interactable.Unhighlight();

            other.TryGetComponent(out StoreBag bag);
            if (bag != null) nearbyBagId = null;
            other.TryGetComponent(out WorldItem item);
            if (item != null) nearbyItem = null;
        }
    }
}