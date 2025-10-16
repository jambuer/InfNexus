using UnityEngine.UIElements;
using GDS.Core;
using GDS.Demos.Basic.Views;
using static GDS.Core.Dom;

namespace GDS.Demos.Basic {

    public class CharacterSheetWindow : VisualElement {
        public CharacterSheetWindow(CharacterSheet character) {

            var Defense = Label("character-line", "Defense");
            var DPS = Label("character-line", "DPS");
            var APS = Label("character-line", "Attack Speed");
            this.Add("window character-sheet gap-v-50",
                Components.CloseButton(character),
                Title("Character Sheet"),
                Div(Defense,
                    DPS,
                    APS
                )
            );

            this.Observe(character.Stats, (data) => {
                Defense.text = $"Defense: {DefenseText(data.Defense)}";
                DPS.text = $"Damage: {DamageText(data.Damage)}" + "/s".DarkGray();
                APS.text = $"Attack Speed: {data.AttackSpeed}" + "/s".DarkGray();
            });

        }

        string DefenseText(int value) => value switch {
            <= 40 => value.ToString().Red(),
            <= 70 => value.ToString().Yellow(),
            _ => value.ToString().Blue()
        };

        string DamageText(float value) => value switch {
            <= 0 => value.ToString().Red(),
            _ => value.ToString().Green()
        };

    }

}
