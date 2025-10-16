using System.Linq;
using GDS.Core;

namespace GDS.Demos.Basic {
    [System.Serializable]
    public class ListItemDto {
        public int index;
        public string baseId;
        public int quant = 1;
        public Rarity rarity = Rarity.NoRarity;
    }

    [System.Serializable]
    public class SetItemDto {
        public string key;
        public string baseId;
        public int quant;
        public Rarity rarity;
    }

    [System.Serializable]
    public class CharacterDto {
        public string playerName;
        public int level;
        public SetItemDto[] equipment;
        public ListItemDto[] inventory;
        public ListItemDto[] hotbar;

        public CharacterDto(string playerName, int level, SetBag equipment, ListBag inventory, ListBag hotbar) {
            this.playerName = playerName;
            this.level = level;
            this.equipment = equipment.Slots.Where(kv => kv.Value.IsFull()).Select(kv => kv.Value.Create()).ToArray();
            this.inventory = inventory.Slots.Where(slot => slot.IsFull()).Select(slot => slot.Create()).ToArray();
            this.hotbar = hotbar.Slots.Where(slot => slot.IsFull()).Select(slot => slot.Create()).ToArray();
        }
    }

    public static class DtoExt {
        public static ListItemDto Create(this ListSlot slot) => new ListItemDto() { index = slot.Index, baseId = slot.Item.Base.Id, quant = slot.Item.Quant, rarity = slot.Item.Rarity() };
        public static SetItemDto Create(this SetSlot slot) => new SetItemDto() { key = slot.Key, baseId = slot.Item.Base.Id, quant = slot.Item.Quant, rarity = slot.Item.Rarity() };

        public static ListSlot Create(this ListItemDto item) => new ListSlot(item.index, ItemFactory.Create(BasesExt.Get(item.baseId), item.rarity, item.quant));
        public static SetSlot Create(this SetItemDto item) => new SetSlot(item.key, ItemFactory.Create(BasesExt.Get(item.baseId), item.rarity, item.quant));

        public static BasicItem CreateItem(this ListItemDto item) => ItemFactory.Create(BasesExt.Get(item.baseId), item.rarity, item.quant);
    }
}