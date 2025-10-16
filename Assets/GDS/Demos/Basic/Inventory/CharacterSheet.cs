using GDS.Core;

namespace GDS.Demos.Basic {
    public class CharacterSheet {
        public record CharacterStats(int Defense, float Damage, float AttackSpeed);
        public Observable<CharacterStats> Stats;
        public CharacterSheet(SetBag bag) {
            Stats = new(new(0, 0, 0));

            bag.Data.OnChange += (slots) => {
                int defense = 0;
                float dps = 0f, aps = 0f;

                foreach (var slot in slots.Values) {
                    if (slot.Item is not BasicItem item) continue;
                    if (item is ArmorItem a) defense += a.Defense;
                    if (item is WeaponItem b) { aps = b.AttackSpeed; dps = b.DPS; }
                }

                Stats.SetValue(new(defense, dps, aps));
            };
        }
    }
}