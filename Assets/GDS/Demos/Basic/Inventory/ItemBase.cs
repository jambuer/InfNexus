namespace GDS.Demos.Basic {

    public class ItemBase : GDS.Core.ItemBase {
        public static readonly NoBase NoBase = new();
        public Class Class = Class.NoClass;
    }

    public class NoBase : ItemBase { }

    public class ArmorBase : ItemBase {
        public Range Defense = new();
    }

    public class WeaponBase : ItemBase {
        public Range Attack = new();
        public float AttackSpeed = 0;
    }
}