namespace LumiLantern
{
    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public enum BeamColor
    {
        White = 0,
        Red = 1,
        Blue = 2
    }

    public enum MirrorType
    {
        None = 0,
        Slash = 1,      // Forward slash /
        Backslash = 2   // Backslash \
    }

    public enum CellType
    {
        Empty,          // .
        Wall,           // #
        SourceRight,    // >
        SourceUp,       // ^
        SourceDown,     // v
        SourceLeft,     // <
        LanternNormal,  // L (lights on any color)
        LanternRed,     // R (lights on red only)
        LanternBlue,    // B (lights on blue only)
        FilterRed,      // r (turns white to red; blocks blue)
        FilterBlue,     // b (turns white to blue; blocks red)
        SplitterSlash,  // P (splits into straight + slash reflection)
        SplitterBackslash, // Q (splits into straight + backslash reflection)
        FixedMirrorSlash,     // m (permanent / mirror)
        FixedMirrorBackslash, // n (permanent \ mirror)
        RhythmGate      // G (closed if placed mirrors count is even, open if odd)
    }

    public enum GameState
    {
        Playing,
        LevelWon,
        ChapterSelect
    }
}
