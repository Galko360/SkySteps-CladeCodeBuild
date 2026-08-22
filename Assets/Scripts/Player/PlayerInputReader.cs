using UnityEngine;
using UnityEngine.InputSystem;

namespace SkySteps.Player
{
    /// <summary>
    /// Adapts the Input System asset into simple movement intent. Nothing downstream references
    /// UnityEngine.InputSystem, so rebinding or swapping the input source stays contained here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";

        [Tooltip("How far the stick or key must push down before it counts as a deliberate drop input.")]
        [SerializeField, Range(0.1f, 0.95f)] private float downThreshold = 0.5f;

        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        // Tracks which actions this component enabled, so teardown never disables an action it
        // did not turn on.
        private bool _enabledMove;
        private bool _enabledJump;

        public float MoveX => _moveAction != null ? _moveAction.ReadValue<Vector2>().x : 0f;
        public bool DownHeld => _moveAction != null && _moveAction.ReadValue<Vector2>().y <= -downThreshold;
        public bool JumpPressedThisFrame => _jumpAction != null && _jumpAction.WasPressedThisFrame();
        public bool JumpHeld => _jumpAction != null && _jumpAction.IsPressed();

        private void Awake()
        {
            if (actionAsset == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader)}: no InputActionAsset assigned.", this);
                enabled = false;
                return;
            }

            _playerMap = actionAsset.FindActionMap(actionMapName, throwIfNotFound: false);
            if (_playerMap == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader)}: action map '{actionMapName}' not found.", this);
                enabled = false;
                return;
            }

            _moveAction = _playerMap.FindAction(moveActionName, throwIfNotFound: false);
            _jumpAction = _playerMap.FindAction(jumpActionName, throwIfNotFound: false);

            if (_moveAction == null || _jumpAction == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerInputReader)}: expected actions '{moveActionName}' and '{jumpActionName}' " +
                    $"in map '{actionMapName}'.",
                    this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _enabledMove = TryEnable(_moveAction);
            _enabledJump = TryEnable(_jumpAction);
        }

        private void OnDisable()
        {
            // Only actions this component switched on are switched back off, so a shared or
            // project-wide asset keeps working for anything else listening to it.
            if (_enabledMove) _moveAction.Disable();
            if (_enabledJump) _jumpAction.Disable();

            _enabledMove = false;
            _enabledJump = false;
        }

        /// <summary>
        /// Enables a single action, reporting whether this call is what turned it on.
        /// Unity 6 auto-enables the project-wide action asset, and calling Enable() on a map that
        /// the Input System already owns trips an internal 'Map must be contained in state' assert.
        /// Enabling per action, only when it is off, is safe for both owned and shared assets.
        /// </summary>
        private static bool TryEnable(InputAction action)
        {
            if (action == null || action.enabled) return false;

            action.Enable();
            return true;
        }
    }
}
