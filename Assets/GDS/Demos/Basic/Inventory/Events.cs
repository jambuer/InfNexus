using GDS.Core;
using GDS.Core.Events;
namespace GDS.Demos.Basic.Events {
    public record ToggleCharacterSheet() : Command;
    public record HotbarUse(int Index) : Command;
    public record CollectAll(ListBag Bag) : Command;
}