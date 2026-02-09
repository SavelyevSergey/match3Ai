using System.Collections.Generic;
using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Manages the game grid with variable size and supports custom shapes
    /// Handles grid initialization, element placement, and neighbor queries
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 originPosition = Vector2.zero;
        
        private Cell[,] grid;
        private Dictionary<Vector2Int, GameElement> elementMap = new Dictionary<Vector2Int, GameElement>();
        
        public int Width => width;
        public int Height => height;
        public Vector2 OriginPosition => originPosition;
        
        /// <summary>
        /// Initialize the grid with given dimensions
        /// </summary>
        public void Initialize(int gridWidth, int gridHeight)
        {
            width = gridWidth;
            height = gridHeight;
            CreateGrid();
        }
        
        /// <summary>
        /// Create the grid data structure
        /// </summary>
        private void CreateGrid()
        {
            grid = new Cell[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Cell(new Vector2Int(x, y));
                }
            }
        }
        
        /// <summary>
        /// Get cell at grid coordinates
        /// </summary>
        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            
            return grid[x, y];
        }
        
        /// <summary>
        /// Get cell at grid coordinates (Vector2Int)
        /// </summary>
        public Cell GetCell(Vector2Int position)
        {
            return GetCell(position.x, position.y);
        }
        
        /// <summary>
        /// Get cell world position
        /// </summary>
        public Vector2 GetWorldPosition(int x, int y)
        {
            return new Vector2(
                originPosition.x + x * cellSize,
                originPosition.y + y * cellSize
            );
        }
        
        /// <summary>
        /// Get cell world position from Vector2Int
        /// </summary>
        public Vector2 GetWorldPosition(Vector2Int gridPos)
        {
            return GetWorldPosition(gridPos.x, gridPos.y);
        }
        
        /// <summary>
        /// Get grid coordinates from world position
        /// </summary>
        public Vector2Int GetGridPosition(Vector2 worldPos)
        {
            int x = Mathf.RoundToInt((worldPos.x - originPosition.x) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - originPosition.y) / cellSize);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Get all available neighbors (4-directional: up, down, left, right)
        /// </summary>
        public List<Cell> GetNeighbors(Cell cell)
        {
            return GetNeighbors(cell.position);
        }
        
        /// <summary>
        /// Get all available neighbors for a grid position (4-directional)
        /// </summary>
        public List<Cell> GetNeighbors(Vector2Int position)
        {
            List<Cell> neighbors = new List<Cell>();
            
            // 4-directional search (no diagonals)
            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = position + dir;
                Cell neighbor = GetCell(neighborPos);
                if (neighbor != null && neighbor.isAvailable)
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Place an element at grid position
        /// </summary>
        public void PlaceElement(GameElement element, Vector2Int position)
        {
            Cell cell = GetCell(position);
            if (cell != null)
            {
                cell.element = element;
                element.SetCell(cell);
                element.transform.position = GetWorldPosition(position);
                element.UpdateGridPosition(position);
                
                if (!elementMap.ContainsKey(position))
                {
                    elementMap[position] = element;
                }
            }
        }
        
        /// <summary>
        /// Remove element from grid position
        /// </summary>
        public void RemoveElement(Vector2Int position)
        {
            Cell cell = GetCell(position);
            if (cell != null)
            {
                cell.element = null;
            }
            
            if (elementMap.ContainsKey(position))
            {
                elementMap.Remove(position);
            }
        }
        
        /// <summary>
        /// Swap two elements on the grid
        /// </summary>
        public void SwapElements(Vector2Int pos1, Vector2Int pos2)
        {
            Cell cell1 = GetCell(pos1);
            Cell cell2 = GetCell(pos2);
            
            if (cell1 == null || cell2 == null)
                return;
            
            GameElement elem1 = cell1.element;
            GameElement elem2 = cell2.element;
            
            // Update cell references
            cell1.element = elem2;
            cell2.element = elem1;
            
            // Update element cell references
            if (elem1 != null) elem1.SetCell(cell2);
            if (elem2 != null) elem2.SetCell(cell1);
            
            // Update world positions
            if (elem1 != null)
            {
                elem1.transform.position = GetWorldPosition(pos2);
                elem1.UpdateGridPosition(pos2);
            }
            if (elem2 != null)
            {
                elem2.transform.position = GetWorldPosition(pos1);
                elem2.UpdateGridPosition(pos1);
            }
            
            // Update element map
            elementMap.Remove(pos1);
            elementMap.Remove(pos2);
            if (elem1 != null) elementMap[pos2] = elem1;
            if (elem2 != null) elementMap[pos1] = elem2;
        }
        
        /// <summary>
        /// Check if position is within grid bounds
        /// </summary>
        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < width && 
                   position.y >= 0 && position.y < height;
        }
        
        /// <summary>
        /// Check if cell is available for gameplay
        /// </summary>
        public bool IsCellAvailable(Vector2Int position)
        {
            Cell cell = GetCell(position);
            return cell != null && cell.isAvailable;
        }
        
        /// <summary>
        /// Get all cells in the grid
        /// </summary>
        public IEnumerable<Cell> GetAllCells()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    yield return grid[x, y];
                }
            }
        }
        
        /// <summary>
        /// Get all elements in the grid
        /// </summary>
        public IEnumerable<GameElement> GetAllElements()
        {
            foreach (var kvp in elementMap)
            {
                if (kvp.Value != null)
                    yield return kvp.Value;
            }
        }
        
        /// <summary>
        /// Clear all elements from the grid
        /// </summary>
        public void ClearGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        grid[x, y].element = null;
                    }
                }
            }
            elementMap.Clear();
        }
        
        /// <summary>
        /// Debug visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(
                        originPosition.x + x * cellSize,
                        originPosition.y + y * cellSize,
                        0
                    );
                    Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
