using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Level configuration data for pre-production prototype
    /// Configures grid size and basic level parameters
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Match3AI/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Grid Configuration")]
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 8;
        [SerializeField] private GridShape _shape = GridShape.Rectangle;
        
        [Header("Level Settings")]
        [SerializeField] private int _moves = 30;
        [SerializeField] private int _targetScore = 1000;
        
        [Header("Element Prefab")]
        [SerializeField] private GameElement _elementPrefab;
        
        public int width => _width;
        public int height => _height;
        public GridShape shape => _shape;
        public int moves => _moves;
        public int targetScore => _targetScore;
        public GameElement elementPrefab => _elementPrefab;
        
        /// <summary>
        /// Validate level configuration
        /// </summary>
        public bool IsValid()
        {
            return width > 0 && height > 0 && elementPrefab != null;
        }
        
        /// <summary>
        /// Get grid dimensions as Vector2Int
        /// </summary>
        public Vector2Int GetGridSize()
        {
            return new Vector2Int(width, height);
        }
    }
}
