using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDS.Core.Views {
    public class DropTargetView : VisualElement {
        public DropTargetView(Observable<Item> dragged, Action<Item> callback) {
            this.WithClass("drop-target drop-target-visible").Hide();
            this.Observe(dragged, item => {
                this.SetVisible(item is not NoItem);
            });
            RegisterCallback<PointerUpEvent>(e => {
                if (e.button != 0) return;
                if (dragged.Value is NoItem) return;
                callback(dragged.Value);
            });

        }
    }
}
