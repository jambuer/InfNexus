using System;
using System.Linq;
using UnityEngine.UIElements;
using GDS.Demos.Basic.Views;
using GDS.Demos.Basic.Events;

using GDS.Core;
using GDS.Core.Views;

namespace GDS.Demos.Basic {

    public class ChestWindow : VisualElement {

        public ChestWindow(ListBag chest, string titleText = "Chest (Remove Only)") {

            Action onCollectClick = () => Store.Bus.Publish(new CollectAll(chest));

            var defaultState = Dom.Div("container",
                new BasicListView(chest),
                Dom.Button("collect-button", "Collect all", onCollectClick)
            );
            var emptyState = Dom.Label("empty-message", "[Empty]");

            this.Add("window chest",
                Components.CloseButton(chest),
                Dom.Title(titleText),
                emptyState,
                defaultState
            );

            this.Observe(chest.Data, (_) => {
                if (chest.IsEmpty()) {
                    defaultState.Hide();
                    emptyState.Show();
                    return;
                }

                emptyState.Hide();
                defaultState.Show();
            });

        }
    }
}
