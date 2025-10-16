using GDS.Core;
using GDS.Core.Events;
using GDS.Demos.Basic.Events;
using UnityEngine;


namespace GDS.Demos.Basic {
    // Plays sounds on certain events
    [RequireComponent(typeof(AudioSource))]
    public class InventorySfx : MonoBehaviour {

        AudioSource audioSource;
        EventBus bus = Store.Bus;

        public SoundList Sounds;

        [System.Serializable]
        public class SoundList {
            public AudioClip Fail;
            public AudioClip Pick;
            public AudioClip Place;
            public AudioClip Move;
            public AudioClip Buy;
            public AudioClip Sell;
            public AudioClip Craft;
        }

        void Awake() {
            audioSource = GetComponent<AudioSource>();
        }

        void OnEnable() {
            bus.On<Fail>(PlayClip);
            bus.On<BagFull>(PlayClip);
            bus.On<CantAfford>(PlayClip);
            bus.On<CantFitAll>(PlayClip);
            bus.On<Restricted<Bag>>(PlayClip);
            bus.On<Restricted<Slot>>(PlayClip);
            bus.On<CollectAll>(PlayClip);

            bus.OnAny<ItemSuccess>(PlayClip);
        }

        void OnDisable() {
            var bus = Store.Bus;
            bus.Off<Fail>(PlayClip);
            bus.Off<BagFull>(PlayClip);
            bus.Off<CantAfford>(PlayClip);
            bus.Off<CantFitAll>(PlayClip);
            bus.Off<Restricted<Bag>>(PlayClip);
            bus.Off<Restricted<Slot>>(PlayClip);
            bus.Off<CollectAll>(PlayClip);

            bus.OffAny<ItemSuccess>(PlayClip);
        }

        AudioClip EventClip(CustomEvent e) => e switch {
            Fail => Sounds.Fail,
            PickItemSuccess => Sounds.Pick,
            PlaceItemSuccess => Sounds.Place,
            MoveItemSuccess => Sounds.Move,
            CraftItemSuccess => Sounds.Craft,
            BuyItemSuccess => Sounds.Buy,
            SellItemSuccess => Sounds.Sell,
            DestroyItemSuccess => Sounds.Place,
            DiscardItemSuccess => Sounds.Place,
            LootItemSuccess => Sounds.Place,
            CollectAll => Sounds.Place,
            _ => null
        };

        void PlayClip(CustomEvent e) {
            var clip = EventClip(e);
            if (clip == null) return;

            audioSource.pitch = Random.Range(0.85f, 1.05f);
            audioSource.PlayOneShot(clip);
        }

    }

}
