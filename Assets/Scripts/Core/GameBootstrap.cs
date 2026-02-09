using UnityEngine;

namespace Match3Ai.Core
{
    /// <summary>
    /// Bootstrap script to automatically setup the prototype scene
    /// Creates all necessary game objects and components
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField] private GameObject elementPrefab;
        
        private void Awake()
        {
            SetupCamera();
            SetupGridManager();
            SetupMatchDetector();
            SetupInputManager();
            SetupGameManager();
        }
        
        /// <summary>
        /// Setup main camera for 2D view
        /// </summary>
        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(3.5f, 3.5f, -10f);
        }
        
        /// <summary>
        /// Setup Grid Manager
        /// </summary>
        private void SetupGridManager()
        {
            GameObject gridObj = new GameObject("GridManager");
            GridManager gridManager = gridObj.AddComponent<GridManager>();
            
            // Create placeholder element prefab if not assigned
            if (elementPrefab == null)
            {
                elementPrefab = CreatePlaceholderElementPrefab();
            }
        }
        
        /// <summary>
        /// Setup Match Detector
        /// </summary>
        private void SetupMatchDetector()
        {
            GameObject detectorObj = new GameObject("MatchDetector");
            detectorObj.AddComponent<MatchDetector>();
        }
        
        /// <summary>
        /// Setup Input Manager
        /// </summary>
        private void SetupInputManager()
        {
            GameObject inputObj = new GameObject("InputManager");
            inputObj.AddComponent<InputManager>();
        }
        
        /// <summary>
        /// Setup Game Manager with all dependencies
        /// </summary>
        private void SetupGameManager()
        {
            GameObject gameObj = new GameObject("GameManager");
            GameManager gameManager = gameObj.AddComponent<GameManager>();
            
            // Setup via reflection or direct reference
            GridManager gridManager = FindObjectOfType<GridManager>();
            MatchDetector matchDetector = FindObjectOfType<MatchDetector>();
            InputManager inputManager = FindObjectOfType<InputManager>();
            
            // Create default level config
            LevelConfig levelConfig = CreateDefaultLevelConfig();
            
            // We need to set references - using serialized field approach
            // The GameManager will find components at runtime
        }
        
        /// <summary>
        /// Create placeholder element prefab with sprite renderer
        /// </summary>
        private GameObject CreatePlaceholderElementPrefab()
        {
            GameObject prefab = new GameObject("ElementPrefab");
            prefab.AddComponent<SpriteRenderer>();
            GameElement element = prefab.AddComponent<GameElement>();
            
            // Create a simple circle sprite
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(0.9f, 0.9f);
            
            // Make it a prefab (not in scene)
            GameObject.DontDestroyOnLoad(prefab);
            prefab.SetActive(false);
            
            return prefab;
        }
        
        /// <summary>
        /// Create a simple circle sprite for placeholder graphics
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            // Create a simple texture for the sprite
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            
            // Draw circle
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Create default level configuration
        /// </summary>
        private LevelConfig CreateDefaultLevelConfig()
        {
            // Create via ScriptableObject (requires UnityEditor for full creation)
            // For runtime, we'll use the GameManager's defaults
            return null;
        }
    }
}
