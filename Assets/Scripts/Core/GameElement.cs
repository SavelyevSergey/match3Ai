using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// MonoBehaviour representing a game element (candy/gem) on the grid
    /// Attached to the GameObject prefab for each element
    /// </summary>
    public class GameElement : MonoBehaviour
    {
        [SerializeField] private ElementType _elementType;
        
        private Cell _currentCell;
        private Vector2Int _gridPosition;
        
        public ElementType elementType => _elementType;
        
        public Cell CurrentCell => _currentCell;
        public Vector2Int GridPosition => _gridPosition;
        
        /// <summary>
        /// Initialize the element with its type and grid position
        /// </summary>
        public void Initialize(ElementType type, Vector2Int gridPos)
        {
            _elementType = type;
            _gridPosition = gridPos;
            UpdateVisualColor();
        }
        
        /// <summary>
        /// Set the current cell this element belongs to
        /// </summary>
        public void SetCell(Cell cell)
        {
            _currentCell = cell;
            if (cell != null)
            {
                cell.element = this;
            }
        }
        
        /// <summary>
        /// Update grid position (called when element moves)
        /// </summary>
        public void UpdateGridPosition(Vector2Int newPos)
        {
            _gridPosition = newPos;
        }
        
        /// <summary>
        /// Update visual color based on element type (placeholder graphics)
        /// </summary>
        private void UpdateVisualColor()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = GetColorForElementType(_elementType);
            }
        }
        
        /// <summary>
        /// Get color for element type (placeholder colors)
        /// </summary>
        private Color GetColorForElementType(ElementType type)
        {
            switch (type)
            {
                case ElementType.Red: return Color.red;
                case ElementType.Orange: return new Color(1f, 0.5f, 0f);
                case ElementType.Yellow: return Color.yellow;
                case ElementType.Green: return Color.green;
                case ElementType.Blue: return Color.blue;
                case ElementType.Purple: return new Color(0.5f, 0f, 1f);
                case ElementType.ColorBomb: return Color.white;
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Set element type (for runtime changes)
        /// </summary>
        public void SetElementType(ElementType type)
        {
            _elementType = type;
            UpdateVisualColor();
        }
    }
}
