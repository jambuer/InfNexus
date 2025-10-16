using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace GDS.Core.Views {
    /// <summary>
    /// Displays a row of buttons. Shows selected element. Has a button click callback.
    /// <para>Requires USS classes: <c>row</c>, <c>tab-button</c>, <c>selected</c></para>
    /// </summary>
    public class ButtonBarView : Component<int> {
        public ButtonBarView(IEnumerable<string> texts, Action<int> callback) {
            Buttons = texts.Select((t, i) => Dom.Button("tab-button", t, () => callback(i))).ToArray();
            this.Add("row", Buttons);
        }

        Button[] Buttons;
        public int Selected { get => Data; set { Data = value; } }

        public override void Render(int index) {
            foreach (var btn in Buttons) btn.WithoutClass("selected");
            Buttons[index].WithClass("selected");
        }
    }
}