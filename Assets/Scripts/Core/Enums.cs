namespace Match3Ai.Core
{
    /// <summary>
    /// Types of game elements (candies/gems) available for matching
    /// </summary>
    public enum ElementType
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        ColorBomb // Special wildcard element
    }
    
    /// <summary>
    /// Grid shape types for level configuration
    /// </summary>
    public enum GridShape
    {
        Rectangle,  // Standard rectangular grid
        Custom      // Custom shape using customCells array
    }
    
    /// <summary>
    /// Cell type for special grid positions
    /// </summary>
    public enum CellType
    {
        Normal,      // Standard playable cell
        Spawner,     // Spawns new elements (used in drop levels)
        Exit         // Target for collecting elements (used in drop levels)
    }
}
