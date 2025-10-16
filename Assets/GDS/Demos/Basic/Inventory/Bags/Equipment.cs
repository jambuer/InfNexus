using System.Linq;
using GDS.Core;

namespace GDS.Demos.Basic {
    public class Equipment : SetBag {

        public static class Keys {
            public static readonly string Helmet = "Helmet", Gloves = "Gloves", Boots = "Boots", BodyArmor = "BodyArmor", Weapon = "Weapon";
            public static readonly string[] All = typeof(Keys).GetFields().Where(f => f.FieldType == typeof(string)).Select(f => (string)f.GetValue(null)).ToArray();
        }

        public Equipment(string id) {
            Id = id;
            Data = new(new() {
                {Keys.Helmet,  SlotFactory.Create(Keys.Helmet, Filters.Helmet)},
                {Keys.Gloves, SlotFactory.Create(Keys.Gloves, Filters.Gloves)},
                {Keys.Boots, SlotFactory.Create(Keys.Boots, Filters.Boots)},
                {Keys.BodyArmor, SlotFactory.Create(Keys.BodyArmor, Filters.BodyArmor)},
                {Keys.Weapon, SlotFactory.Create(Keys.Weapon, Filters.Weapon)},
            });

            // Assert all keys are in dictionary
            foreach (var key in Keys.All)
                if (!Data.Value.ContainsKey(key)) throw new System.Exception($"Missing equipment key in dictionary: {key}");

        }
    }

}