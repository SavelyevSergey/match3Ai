using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Represents a single cell in the game grid
    /// Contains information about position, element, and cell state
    /// </summary>
    public class Cell
    {
        public Vector2Int position;
        public GameElement element;
        public bool isAvailable;
        public CellType cellType;
        
        public Cell(Vector2Int pos)
        {
            position = pos;
            element = null;
            isAvailable = true;
            cellType = CellType.Normal;
        }
        
        public Cell(Vector2Int pos, bool available, CellType type = CellType.Normal)
        {
            position = pos;
            element = null;
            isAvailable = available;
            cellType = type;
        }
        
        /// <summary>
        /// Check if this cell is empty (no element)
        /// </summary>
        public bool IsEmpty => element == null;
        
        /// <summary>
        /// Check if cell contains a matchable element
        /// </summary>
        public bool HasElement => element != null;
        
        /// <summary>
        /// Get the type of element in this cell, or null if empty
        /// </summary>
        public ElementType? ElementType => element?.elementType;
        
        public override string ToString()
        {
            return $"Cell({position.x}, {position.y}) - {(IsEmpty ? "Empty" : element?.elementType.ToString())}";
        }
    }
}
