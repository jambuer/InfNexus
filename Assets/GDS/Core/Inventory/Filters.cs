using System;

namespace GDS.Core {
    /// <summary>
    /// Provides common filter predicates for <see cref="Item"/> collections.
    /// Includes filters that match all items or no items.
    /// </summary>
    public static class Filters {
        /// <summary>
        /// A filter that matches all items.
        /// </summary>
        public static readonly Predicate<Item> Everything = _ => true;

        /// <summary>
        /// A filter that matches no items.
        /// </summary>
        public static readonly Predicate<Item> Nothing = _ => false;

    }
    public delegate object SortFn(Item item);

}