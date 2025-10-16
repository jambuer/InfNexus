namespace GDS.Core.Views {
    public interface ISlotView {
        public Bag Bag { get; }
        public Slot Slot { get; }
    }

    public delegate Component<Slot> CreateSlotFn(Slot slot);

    public class SlotView : Component<Slot>, ISlotView {

        public Slot Slot { get => Data; }
        public Bag Bag { get; }
        Component<Item> ItemView;

        public SlotView(Bag bag, Slot slot, CreateItemFn createItem = null) {
            ItemView = createItem is null ? new ItemView() : createItem();
            this.Add("slot", ItemView, Dom.Div("cover overlay")).PickIgnoreChildren();
            Bag = bag;
            Data = slot;
        }

        public override void Render(Slot slot) {
            ItemView.Data = slot.Item;
            EnableInClassList("empty", slot.IsEmpty());
        }
    }
}