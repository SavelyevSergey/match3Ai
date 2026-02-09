using UnityEngine;
using System;

namespace Match3Ai.Core
{
    /// <summary>
    /// Direction of swipe input
    /// </summary>
    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    
    /// <summary>
    /// Handles player input for swipe-based gameplay
    /// Supports both mouse and touch input
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isSwiping = false;
        
        private GridManager gridManager;
        
        public event Action<Cell, Cell> OnSwapAttempt;
        public event Action OnInputCancelled;
        
        public void Initialize(GridManager manager)
        {
            gridManager = manager;
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        /// <summary>
        /// Handle mouse/touch input
        /// </summary>
        private void HandleInput()
        {
            // Touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                HandleTouch(touch);
            }
            // Mouse input (for testing in editor)
            else if (Input.GetMouseButtonDown(0))
            {
                HandleMouseDown();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleMouseUp();
            }
        }
        
        /// <summary>
        /// Handle touch input
        /// </summary>
        private void HandleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartTouch(touch.position);
                    break;
                    
                case TouchPhase.Ended:
                    EndTouch(touch.position);
                    break;
            }
        }
        
        /// <summary>
        /// Handle mouse button down
        /// </summary>
        private void HandleMouseDown()
        {
            StartTouch(Input.mousePosition);
        }
        
        /// <summary>
        /// Handle mouse button up
        /// </summary>
        private void HandleMouseUp()
        {
            EndTouch(Input.mousePosition);
        }
        
        /// <summary>
        /// Record touch/mouse start position and time
        /// </summary>
        private void StartTouch(Vector2 position)
        {
            touchStartPosition = position;
            touchStartTime = Time.time;
            isSwiping = true;
        }
        
        /// <summary>
        /// Process touch/mouse end and determine swipe direction
        /// </summary>
        private void EndTouch(Vector2 position)
        {
            if (!isSwiping)
                return;
            
            isSwiping = false;
            
            Vector2 touchEndPosition = position;
            float touchDuration = Time.time - touchStartTime;
            
            // Calculate swipe vector
            Vector2 swipeVector = touchEndPosition - touchStartPosition;
            float swipeDistance = swipeVector.magnitude;
            
            // Check if swipe meets minimum requirements
            if (swipeDistance < minSwipeDistance || touchDuration > maxSwipeTime)
            {
                OnInputCancelled?.Invoke();
                return;
            }
            
            // Determine swipe direction
            SwipeDirection direction = GetSwipeDirection(swipeVector);
            
            if (direction != SwipeDirection.None)
            {
                ProcessSwipe(direction);
            }
        }
        
        /// <summary>
        /// Determine swipe direction from swipe vector
        /// </summary>
        private SwipeDirection GetSwipeDirection(Vector2 swipeVector)
        {
            float x = swipeVector.x;
            float y = swipeVector.y;
            
            // Determine primary direction
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // Horizontal swipe
                return x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                // Vertical swipe
                return y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
        
        /// <summary>
        /// Process swipe and attempt swap
        /// </summary>
        private void ProcessSwipe(SwipeDirection direction)
        {
            if (gridManager == null)
                return;
            
            // Get the cell under the touch/mouse position
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touchStartPosition);
            Vector2Int gridPos = gridManager.GetGridPosition(worldPos);
            
            Cell startCell = gridManager.GetCell(gridPos);
            if (startCell == null || !startCell.HasElement)
            {
                OnInputCancelled?.Invoke();
                return;
            }
            
            // Calculate target cell based on swipe direction
            Vector2Int targetPos = GetTargetPosition(gridPos, direction);
            Cell targetCell = gridManager.GetCell(targetPos);
            
            if (targetCell != null && targetCell.HasElement)
            {
                OnSwapAttempt?.Invoke(startCell, targetCell);
            }
            else
            {
                OnInputCancelled?.Invoke();
            }
        }
        
        /// <summary>
        /// Get target grid position based on swipe direction
        /// </summary>
        private Vector2Int GetTargetPosition(Vector2Int startPos, SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.Up:
                    return startPos + Vector2Int.up;
                case SwipeDirection.Down:
                    return startPos + Vector2Int.down;
                case SwipeDirection.Left:
                    return startPos + Vector2Int.left;
                case SwipeDirection.Right:
                    return startPos + Vector2Int.right;
                default:
                    return startPos;
            }
        }
        
        /// <summary>
        /// Convert screen pixels to world position
        /// </summary>
        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return Camera.main != null ? Camera.main.ScreenToWorldPoint(screenPos) : (Vector2)screenPos;
        }
    }
}
