using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Main game states according to GDD
    /// </summary>
    public enum GameState
    {
        Initialization,
        Idle,
        PlayerInput,
        SwapAnimation,
        ValidatingSwap,
        InvalidSwap,
        MatchChecking,
        Destroying,
        Falling,
        Spawning,
        CascadeCheck,
        BonusCalculation,
        LevelComplete,
        LevelFailed,
        GameOver
    }
    
    /// <summary>
    /// Main game manager handling state machine and core gameplay loop
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private LevelConfig levelConfig;
        
        [Header("Timing")]
        [SerializeField] private float swapDuration = 0.3f;
        [SerializeField] private float destroyDuration = 0.4f;
        [SerializeField] private float fallDuration = 0.5f;
        [SerializeField] private float spawnDuration = 0.3f;
        [SerializeField] private float cascadeDelay = 0.1f;
        
        private GameState currentState;
        private int movesLeft;
        private int score;
        private bool isProcessingMove = false;
        
        public GameState CurrentState => currentState;
        public int MovesLeft => movesLeft;
        public int Score => score;
        
        // Events
        public event System.Action<GameState> OnStateChanged;
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnMovesChanged;
        public event System.Action OnLevelComplete;
        public event System.Action OnLevelFailed;
        
        private void Start()
        {
            InitializeGame();
        }
        
        /// <summary>
        /// Initialize the game with level configuration
        /// </summary>
        private void InitializeGame()
        {
            ChangeState(GameState.Initialization);
            
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();
            if (matchDetector == null)
                matchDetector = FindObjectOfType<MatchDetector>();
            if (inputManager == null)
                inputManager = FindObjectOfType<InputManager>();
            
            matchDetector.Initialize(gridManager);
            inputManager.Initialize(gridManager);
            inputManager.OnSwapAttempt += HandleSwapAttempt;
            
            // Setup level
            if (levelConfig != null)
            {
                gridManager.Initialize(levelConfig.width, levelConfig.height);
                GenerateLevel();
            }
            else
            {
                // Default 8x8 level
                gridManager.Initialize(8, 8);
                GenerateDefaultLevel();
            }
            
            movesLeft = 30; // Default moves for prototype
            score = 0;
            
            OnMovesChanged?.Invoke(movesLeft);
            OnScoreChanged?.Invoke(score);
            
            ChangeState(GameState.Idle);
        }
        
        /// <summary>
        /// Generate level based on configuration
        /// </summary>
        private void GenerateLevel()
        {
            GenerateGridElements(false);
        }
        
        /// <summary>
        /// Generate default 8x8 level
        /// </summary>
        private void GenerateDefaultLevel()
        {
            GenerateGridElements(false);
        }
        
        /// <summary>
        /// Generate random elements on the grid
        /// </summary>
        private void GenerateGridElements(bool preventInitialMatches)
        {
            GameElement prefab = levelConfig?.elementPrefab;
            
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    ElementType type = GetRandomElementType();
                    
                    // Prevent initial matches
                    if (preventInitialMatches)
                    {
                        int attempts = 0;
                        while (WouldCreateInitialMatch(pos, type) && attempts < 10)
                        {
                            type = GetRandomElementType();
                            attempts++;
                        }
                    }
                    
                    CreateElement(prefab, pos, type);
                }
            }
        }
        
        /// <summary>
        /// Check if adding element at position would create a match
        /// </summary>
        private bool WouldCreateInitialMatch(Vector2Int pos, ElementType type)
        {
            Cell cell = gridManager.GetCell(pos);
            if (cell == null)
                return false;
            
            // Check horizontal
            Cell left1 = gridManager.GetCell(pos.x - 1, pos.y);
            Cell left2 = gridManager.GetCell(pos.x - 2, pos.y);
            if (left1 != null && left2 != null && 
                left1.HasElement && left2.HasElement &&
                left1.ElementType == type && left2.ElementType == type)
            {
                return true;
            }
            
            // Check vertical
            Cell down1 = gridManager.GetCell(pos.x, pos.y - 1);
            Cell down2 = gridManager.GetCell(pos.x, pos.y - 2);
            if (down1 != null && down2 != null && 
                down1.HasElement && down2.HasElement &&
                down1.ElementType == type && down2.ElementType == type)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get random element type (excluding ColorBomb for basic gameplay)
        /// </summary>
        private ElementType GetRandomElementType()
        {
            return (ElementType)Random.Range(0, 6); // Red to Purple
        }
        
        /// <summary>
        /// Create an element at grid position (with fallback for asset-less mode)
        /// </summary>
        private void CreateElement(GameElement prefab, Vector2Int pos, ElementType type)
        {
            GameElement element;
            
            if (prefab != null)
            {
                element = Instantiate(prefab, gridManager.GetWorldPosition(pos), Quaternion.identity);
            }
            else
            {
                // Fallback: create a primitive GameObject with GameElement component
                element = CreatePlaceholderElement(pos, type);
            }
            
            element.Initialize(type, pos);
            element.SetCell(gridManager.GetCell(pos));
            gridManager.PlaceElement(element, pos);
        }
        
        /// <summary>
        /// Create a placeholder element using primitive Unity shapes
        /// Used when no prefab assets are available
        /// </summary>
        private GameElement CreatePlaceholderElement(Vector2Int pos, ElementType type)
        {
            // Create a primitive cube as the base object
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Element_{type}_{pos.x}_{pos.y}";
            go.transform.position = gridManager.GetWorldPosition(pos);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            // Remove collider for gameplay elements (they're managed by grid)
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            
            // Add GameElement component
            GameElement element = go.AddComponent<GameElement>();
            
            // Set color based on element type
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetColorForElementType(type);
            }
            
            return element;
        }
        
        /// <summary>
        /// Get color for element type (shared with GameElement)
        /// </summary>
        private Color GetColorForElementType(ElementType type)
        {
            switch (type)
            {
                case ElementType.Red: return new Color(0.9f, 0.2f, 0.2f);
                case ElementType.Orange: return new Color(1f, 0.5f, 0f);
                case ElementType.Yellow: return new Color(1f, 0.9f, 0.2f);
                case ElementType.Green: return new Color(0.2f, 0.8f, 0.2f);
                case ElementType.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case ElementType.Purple: return new Color(0.6f, 0.2f, 0.8f);
                case ElementType.ColorBomb: return new Color(1f, 1f, 1f);
                default: return Color.gray;
            }
        }
        
        /// <summary>
        /// Handle swap attempt from input
        /// </summary>
        private void HandleSwapAttempt(Cell cell1, Cell cell2)
        {
            if (currentState != GameState.Idle || isProcessingMove)
                return;
            
            StartCoroutine(ProcessSwap(cell1, cell2));
        }
        
        /// <summary>
        /// Process the swap and match cascade
        /// </summary>
        private IEnumerator ProcessSwap(Cell cell1, Cell cell2)
        {
            ChangeState(GameState.SwapAnimation);
            isProcessingMove = true;
            
            // Perform swap
            gridManager.SwapElements(cell1.position, cell2.position);
            
            yield return new WaitForSeconds(swapDuration);
            
            // Check if swap created a match
            ChangeState(GameState.ValidatingSwap);
            bool hasMatch = matchDetector.WouldCreateMatch(cell1, cell2);
            
            if (!hasMatch)
            {
                // Invalid swap - return elements
                ChangeState(GameState.InvalidSwap);
                gridManager.SwapElements(cell1.position, cell2.position);
                
                yield return new WaitForSeconds(swapDuration);
                
                ChangeState(GameState.Idle);
                isProcessingMove = false;
                yield break;
            }
            
            // Valid swap - process matches
            movesLeft--;
            OnMovesChanged?.Invoke(movesLeft);
            
            // Process all matches and cascades
            yield return StartCoroutine(ProcessMatches());
            
            // Check win/lose conditions
            CheckGameState();
            
            if (currentState == GameState.Idle)
            {
                isProcessingMove = false;
            }
        }
        
        /// <summary>
        /// Process all matches and cascade effects
        /// </summary>
        private IEnumerator ProcessMatches()
        {
            while (true)
            {
                ChangeState(GameState.MatchChecking);
                
                List<MatchResult> matches = matchDetector.FindAllMatches();
                if (matches.Count == 0)
                    break;
                
                // Process matches
                ChangeState(GameState.Destroying);
                foreach (var match in matches)
                {
                    score += match.Count * 10;
                    foreach (var cell in match.cells)
                    {
                        if (cell.HasElement)
                        {
                            Destroy(cell.element.gameObject);
                            gridManager.RemoveElement(cell.position);
                        }
                    }
                }
                OnScoreChanged?.Invoke(score);
                
                yield return new WaitForSeconds(destroyDuration);
                
                // Drop elements
                ChangeState(GameState.Falling);
                yield return StartCoroutine(DropElements());
                
                // Spawn new elements
                ChangeState(GameState.Spawning);
                yield return StartCoroutine(SpawnNewElements());
                
                yield return new WaitForSeconds(cascadeDelay);
            }
            
            ChangeState(GameState.CascadeCheck);
        }
        
        /// <summary>
        /// Drop elements to fill empty spaces
        /// </summary>
        private IEnumerator DropElements()
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 1; y < gridManager.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Cell cell = gridManager.GetCell(pos);
                    
                    if (cell == null || !cell.HasElement)
                        continue;
                    
                    // Find lowest empty position below
                    int dropDistance = 0;
                    for (int checkY = y - 1; checkY >= 0; checkY--)
                    {
                        Cell checkCell = gridManager.GetCell(x, checkY);
                        if (checkCell == null || !checkCell.HasElement)
                        {
                            dropDistance = y - checkY;
                            break;
                        }
                    }
                    
                    if (dropDistance > 0)
                    {
                        Vector2Int newPos = new Vector2Int(x, y - dropDistance);
                        Cell newCell = gridManager.GetCell(newPos);
                        
                        if (newCell != null)
                        {
                            gridManager.RemoveElement(pos);
                            gridManager.PlaceElement(cell.element, newPos);
                            
                            // Animate drop
                            float elapsed = 0;
                            Vector3 startPos = gridManager.GetWorldPosition(pos);
                            Vector3 endPos = gridManager.GetWorldPosition(newPos);
                            
                            while (elapsed < fallDuration)
                            {
                                cell.element.transform.position = Vector3.Lerp(startPos, endPos, elapsed / fallDuration);
                                elapsed += Time.deltaTime;
                                yield return null;
                            }
                            cell.element.transform.position = endPos;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Spawn new elements at the top (with fallback for asset-less mode)
        /// </summary>
        private IEnumerator SpawnNewElements()
        {
            GameElement prefab = levelConfig?.elementPrefab;
            
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Cell cell = gridManager.GetCell(x, y);
                    
                    if (cell != null && !cell.HasElement)
                    {
                        ElementType type = GetRandomElementType();
                        
                        // Spawn above the grid and animate down
                        Vector2Int pos = new Vector2Int(x, y);
                        Vector3 spawnPos = gridManager.GetWorldPosition(x, gridManager.Height + y - 1);
                        
                        GameElement element;
                        if (prefab != null)
                        {
                            element = Instantiate(prefab, spawnPos, Quaternion.identity);
                        }
                        else
                        {
                            // Fallback: create a placeholder element
                            element = CreatePlaceholderElement(pos, type);
                            element.transform.position = spawnPos;
                        }
                        
                        element.Initialize(type, pos);
                        
                        float elapsed = 0;
                        Vector3 startPos = element.transform.position;
                        Vector3 endPos = gridManager.GetWorldPosition(pos);
                        
                        while (elapsed < spawnDuration)
                        {
                            element.transform.position = Vector3.Lerp(startPos, endPos, elapsed / spawnDuration);
                            elapsed += Time.deltaTime;
                            yield return null;
                        }
                        element.transform.position = endPos;
                        
                        gridManager.PlaceElement(element, pos);
                    }
                }
            }
        }
        
        /// <summary>
        /// Check win/lose conditions
        /// </summary>
        private void CheckGameState()
        {
            // Check for possible moves
            if (!matchDetector.HasPossibleMoves() && movesLeft > 0)
            {
                // Deadlock - shuffle needed (simplified: just end level for prototype)
                Debug.Log("Deadlock detected!");
            }
            
            // Check lose condition
            if (movesLeft <= 0 && currentState == GameState.Idle)
            {
                ChangeState(GameState.LevelFailed);
                OnLevelFailed?.Invoke();
                return;
            }
            
            // For prototype: win when score reaches 1000
            if (score >= 1000)
            {
                ChangeState(GameState.LevelComplete);
                OnLevelComplete?.Invoke();
                return;
            }
            
            ChangeState(GameState.Idle);
        }
        
        /// <summary>
        /// Change game state
        /// </summary>
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            Debug.Log($"State changed to: {newState}");
        }
        
        private void OnDestroy()
        {
            if (inputManager != null)
            {
                inputManager.OnSwapAttempt -= HandleSwapAttempt;
            }
        }
    }
}
