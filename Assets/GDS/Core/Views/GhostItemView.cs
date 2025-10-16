namespace GDS.Core.Views {

    public class GhostItemView : Component<Item> {
        public GhostItemView() {
            this.Add("ghost-item").PickIgnore();
        }

        public override void Render(Item item) {
            Clear();
            Add(Dom.Label("white", item.Name));
        }
    }
}