namespace GDS.Core {
    public enum Direction { N = 0, E = 90, S = 180, W = 270 }
    public record Size(int W, int H);
    public record Pos(int X, int Y) { public static Pos NoPos = new(-1, -1); };
}