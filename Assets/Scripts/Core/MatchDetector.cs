using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Result of match detection containing matched cells and their shape
    /// </summary>
    public class MatchResult
    {
        public List<Cell> cells = new List<Cell>();
        public ElementType elementType;
        public bool IsValid => cells.Count >= 3;
        
        public int Count => cells.Count;
        
        public MatchResult(ElementType type)
        {
            elementType = type;
        }
        
        public void AddCell(Cell cell)
        {
            cells.Add(cell);
        }
    }
    
    /// <summary>
    /// Detects matches on the grid using BFS algorithm
    /// Implements 4-directional search (up, down, left, right)
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        [Header("Match Settings")]
        [SerializeField] private int minMatchLength = 3;
        
        private GridManager gridManager;
        
        public void Initialize(GridManager manager)
        {
            gridManager = manager;
        }
        
        /// <summary>
        /// Find all matches on the entire grid
        /// </summary>
        public List<MatchResult> FindAllMatches()
        {
            List<MatchResult> matchedGroups = new List<MatchResult>();
            HashSet<Cell> visited = new HashSet<Cell>();
            
            foreach (var cell in gridManager.GetAllCells())
            {
                if (cell == null || !cell.HasElement || visited.Contains(cell))
                    continue;
                
                MatchResult group = BFS_Search(cell, visited);
                if (group != null && group.Count >= minMatchLength)
                {
                    matchedGroups.Add(group);
                }
            }
            
            return matchedGroups;
        }
        
        /// <summary>
        /// BFS algorithm to find connected elements of the same type
        /// </summary>
        private MatchResult BFS_Search(Cell startCell, HashSet<Cell> visited)
        {
            if (startCell == null || !startCell.HasElement)
                return null;
            
            ElementType targetType = startCell.ElementType.Value;
            Queue<Cell> queue = new Queue<Cell>();
            MatchResult group = new MatchResult(targetType);
            
            queue.Enqueue(startCell);
            
            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                
                if (current == null || visited.Contains(current))
                    continue;
                
                // Check if element type matches
                if (!current.HasElement || current.ElementType != targetType)
                    continue;
                
                visited.Add(current);
                group.AddCell(current);
                
                // Check neighbors (4-directional)
                List<Cell> neighbors = gridManager.GetNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.HasElement && 
                        neighbor.ElementType == targetType && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            return group.Count >= minMatchLength ? group : null;
        }
        
        /// <summary>
        /// Check if a swap would create a match
        /// Used for validating player moves
        /// </summary>
        public bool WouldCreateMatch(Cell cell1, Cell cell2)
        {
            // Temporarily swap elements
            GameElement elem1 = cell1.element;
            GameElement elem2 = cell2.element;
            
            cell1.element = elem2;
            cell2.element = elem1;
            
            bool hasMatch = CheckForMatchesAt(cell1) || CheckForMatchesAt(cell2);
            
            // Swap back
            cell1.element = elem1;
            cell2.element = elem2;
            
            return hasMatch;
        }
        
        /// <summary>
        /// Check for matches at specific cells after a swap
        /// </summary>
        private bool CheckForMatchesAt(Cell cell)
        {
            if (cell == null || !cell.HasElement)
                return false;
            
            ElementType type = cell.ElementType.Value;
            List<Cell> matched = new List<Cell>();
            HashSet<Cell> visited = new HashSet<Cell>();
            
            return BFS_HasMatchAt(cell, visited, type);
        }
        
        /// <summary>
        /// BFS to check if a specific cell is part of a match
        /// </summary>
        private bool BFS_HasMatchAt(Cell startCell, HashSet<Cell> visited, ElementType targetType)
        {
            Queue<Cell> queue = new Queue<Cell>();
            int matchCount = 0;
            
            queue.Enqueue(startCell);
            
            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                
                if (current == null || visited.Contains(current))
                    continue;
                
                if (!current.HasElement || current.ElementType != targetType)
                    continue;
                
                visited.Add(current);
                matchCount++;
                
                List<Cell> neighbors = gridManager.GetNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.HasElement && 
                        neighbor.ElementType == targetType && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            return matchCount >= minMatchLength;
        }
        
        /// <summary>
        /// Check if there are any possible moves left on the grid
        /// Used for deadlock detection
        /// </summary>
        public bool HasPossibleMoves()
        {
            foreach (var cell in gridManager.GetAllCells())
            {
                if (cell == null || !cell.HasElement)
                    continue;
                
                List<Cell> neighbors = gridManager.GetNeighbors(cell);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.HasElement)
                    {
                        // Simulate swap
                        GameElement elem1 = cell.element;
                        GameElement elem2 = neighbor.element;
                        
                        cell.element = elem2;
                        neighbor.element = elem1;
                        
                        bool hasMatch = CheckForMatchesAt(cell) || CheckForMatchesAt(neighbor);
                        
                        // Swap back
                        cell.element = elem1;
                        neighbor.element = elem2;
                        
                        if (hasMatch)
                            return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check for immediate matches (used during level generation)
        /// Returns true if any matches are found
        /// </summary>
        public bool HasImmediateMatches()
        {
            List<MatchResult> matches = FindAllMatches();
            return matches.Count > 0;
        }
    }
}
