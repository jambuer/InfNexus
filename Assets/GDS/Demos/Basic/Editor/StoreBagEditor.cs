using UnityEditor;
using UnityEngine.UIElements;
using GDS.Core;
using System.Collections.Generic;

namespace GDS.Demos.Basic {

    [CustomEditor(typeof(StoreBag))]
    public class StoreBagEditor : Editor {
        public override VisualElement CreateInspectorGUI() {
            StoreBag component = (StoreBag)target;
            List<Bag> bags = component.Bags;

            if (bags == null || bags.Count == 0) return Dom.Label("⚠ Bag list empty!".Yellow());

            var popup = new PopupField<Bag>("Store bag", bags, 0);
            popup.value = bags.Find(b => b.Id == component.BagId) ?? bags[0];
            popup.RegisterCallback<ChangeEvent<Bag>>((evt) => component.BagId = evt.newValue.Id);

            return popup;
        }
    }
}