using GDS.Core;
using GDS.Core.Events;

namespace GDS.Common.Events {
    public record LootWorldItem(IWorldItem WorldItem) : CustomEvent;

    public record PickVicinityItem(Item Item) : Command;
}