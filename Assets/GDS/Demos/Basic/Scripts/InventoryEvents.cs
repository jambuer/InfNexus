using UnityEngine;
using GDS.Core.Events;
using GDS.Common.Scripts;
using GDS.Common.Events;
using GDS.Common;

namespace GDS.Demos.Basic {
    /// <summary>
    /// Listens to pick and drop item events.
    /// When an item is dropped from the inventory it will be created as a world item in a random spot on a circle around the player.
    /// Requires player position (Defaults to world center).
    /// </summary>
    public class InventoryEvents : MonoBehaviour {

        public Transform Player;
        public GameObject ItemPrefab;
        [Range(0, 5)]
        public float DropRadius = 2;
        public Vector3 DropOffset = new Vector3(0, 0.5f, 0);
        public ParticleSystem PickupVFX;

        private void OnEnable() {
            EventBus.Global.On<Reset>(OnReset);
            EventBus.Global.On<LootWorldItem>(OnLootWorldItem);
            Store.Bus.On<DiscardItemSuccess>(OnDropItemSuccess);
        }

        private void OnDisable() {
            EventBus.Global.Off<Reset>(OnReset);
            EventBus.Global.Off<LootWorldItem>(OnLootWorldItem);
            Store.Bus.Off<DiscardItemSuccess>(OnDropItemSuccess);
        }

        void OnReset(CustomEvent e) {
            var items = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
            foreach (var item in items) Destroy(item.gameObject);
        }

        void OnLootWorldItem(CustomEvent e) {
            if (e is not LootWorldItem evt) return;
            if (Store.Instance.UiOpen.Value == true) return;

            var result = Store.Instance.LootItem(evt.WorldItem.Item);

            if (result is not Success) return;
            Destroy(evt.WorldItem.GameObject);
            if (PickupVFX == null) return;
            Instantiate(PickupVFX, evt.WorldItem.GameObject.transform.position, Quaternion.identity);
        }

        void OnDropItemSuccess(CustomEvent e) {
            if (e is not DiscardItemSuccess ev) return;

            Vector3 playerPosition = new Vector3();
            if (Player != null) playerPosition = new Vector3(Player.position.x, 0, Player.position.z);
            var pos = playerPosition + RandomPointOnCircle(DropRadius) + DropOffset;
            var instance = Instantiate(ItemPrefab, pos, Quaternion.identity);
            instance.GetComponent<IWorldItem>().Item = ev.Item;
        }

        Vector3 RandomPointOnCircle(float radius) {
            float angle = Random.Range(0, 360);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            return new Vector3(x, 0, z);
        }
    }

}