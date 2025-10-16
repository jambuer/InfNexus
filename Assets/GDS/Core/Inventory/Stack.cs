namespace GDS.Core {
    public class Stack {
        public static NoStack NoStack = new();
        public static InfiniteStack Infinite = new();
        public Stack(ushort max) => Max = max;
        public ushort Max;
    }
    public class NoStack : Stack { public NoStack() : base(0) { } }
    public class InfiniteStack : Stack { public InfiniteStack(ushort max = 1000) : base(max) { } }
}