using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GDS.Core;
using GDS.Core.Events;
using GDS.Common.Scripts;
using GDS.Demos.Basic.Views;

namespace GDS.Demos.Basic {

    /// <summary>    
    /// Adds a bag to a world interactible object    
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldBag : BagBase {
        [Tooltip("Must be unique")]
        [SerializeField] string Id;
        [SerializeField] BagType BagType = BagType.Chest;
        [SerializeField][Range(1, 80)] int Size = 10;
        [Space(10)]
        [SerializeReference] List<ListItemDto> Items;
        public ListBag Bag { get; private set; }

        ListBag CreateBag(BagType bagType, string id, int size) => bagType switch {
            BagType.Chest => ListBagFactory.Create<Chest>(id, size),
            BagType.Stash => ListBagFactory.Create<Stash>(id, size),
            BagType.Shop => ListBagFactory.Create<Shop>(id, size),
            _ => null
        };

        void OnEnable() {
            EventBus.Global.On<Reset>(Init);
        }
        void OnDisable() {
            EventBus.Global.Off<Reset>(Init);
        }

        void Awake() {
            if (Id == "") Id = GenId();
            Bag = CreateBag(BagType, Id, Size);
            Init(CustomEvent.NoEvent);
        }

        void Init(CustomEvent _) {
            if (Bag == null) return;
            Bag.SetState(Items.Select(DtoExt.CreateItem).ToArray());
        }

        void OnMouseDown() { Store.Bus.Publish(new OpenWindow(Bag)); }


        string GenId() => "Interactible-" + UnityEngine.Random.Range(0, 10000);

    }


}
