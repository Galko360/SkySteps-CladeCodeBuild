using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Cameras
{
    /// <summary>
    /// Follows the player with an orthographic camera and keeps the view inside
    /// <see cref="CameraBounds"/>. Horizontal movement is followed continuously. Vertically the camera
    /// holds the height the player last landed at, so it doesn't bob with every jump, and only follows
    /// while airborne once the player rises or falls too far from that height.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Tooltip("Tells the camera when the target is standing on a floor, which is when it re-centres vertically.")]
        [SerializeField] private PlayerContactProbe targetGround;

        [Tooltip("Optional. When set, the camera never shows anything outside this area.")]
        [SerializeField] private CameraBounds bounds;

        [Tooltip("Optional. Adds a shake on top of the follow; the shaken view still stays inside the bounds.")]
        [SerializeField] private DamageCameraShake shake;

        [Header("Framing")]
        [Tooltip("Where the target sits relative to the screen centre. Positive Y shows more of the level above.")]
        [SerializeField] private Vector2 framingOffset = new Vector2(0f, 1f);

        [Tooltip("The target is never allowed closer than this to the edge of the view, however fast it " +
                 "moves. The map bounds still take priority at the edges of the level.")]
        [SerializeField, Min(0f)] private float minEdgeMargin = 1.5f;

        [Header("Smoothing (seconds to catch up)")]
        [SerializeField, Min(0f)] private float horizontalSmoothTime = 0.15f;
        [SerializeField, Min(0f)] private float verticalSmoothTime = 0.25f;

        [Header("Vertical Follow While Airborne")]
        [Tooltip("How far above its last landing height the target can rise before the camera follows. " +
                 "Just over one full jump keeps single jumps still while a double jump pulls the camera up.")]
        [SerializeField, Min(0f)] private float maxRiseBeforeFollow = 3.5f;

        [Tooltip("How far below its last landing height the target can fall before the camera follows.")]
        [SerializeField, Min(0f)] private float maxFallBeforeFollow = 1f;

        private Camera _camera;

        // Where the camera sits before any shake. Smoothing works from this rather than from the
        // transform, so a shake never feeds back into the follow and the camera settles back exactly.
        private Vector2 _restPosition;
        private float _anchorY;
        private float _velocityX;
        private float _velocityY;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            if (target == null || targetGround == null)
            {
                Debug.LogError($"{nameof(CameraFollow2D)}: target and targetGround must both be assigned.", this);
                enabled = false;
                return;
            }

            if (!_camera.orthographic)
            {
                Debug.LogWarning($"{nameof(CameraFollow2D)}: view-size maths assumes an orthographic camera.", this);
            }
        }

        private void Start()
        {
            SnapToTarget();
        }

        /// <summary>Moves straight to the target with no smoothing, e.g. after a respawn.</summary>
        public void SnapToTarget()
        {
            _anchorY = target.position.y;
            _velocityX = 0f;
            _velocityY = 0f;
            _restPosition = ClampToBounds(GoalPosition());
            SetPosition(_restPosition);
        }

        private void LateUpdate()
        {
            UpdateAnchor();

            Vector2 goal = ClampToBounds(GoalPosition());
            Vector2 next = new Vector2(
                Mathf.SmoothDamp(_restPosition.x, goal.x, ref _velocityX, horizontalSmoothTime),
                Mathf.SmoothDamp(_restPosition.y, goal.y, ref _velocityY, verticalSmoothTime));

            // Smoothing lags a fast-moving target (a max-speed fall trails by several units), so the
            // target is pushed back inside the edge margin first, and the map bounds get the last word.
            _restPosition = ClampToBounds(KeepTargetInView(next));

            // Clamped again after shaking, so a shake near the map edge cannot show beyond it.
            Vector2 shaken = shake != null ? _restPosition + shake.Offset : _restPosition;
            SetPosition(ClampToBounds(shaken));
        }

        /// <summary>
        /// Holds the camera at the height the target last landed at. While airborne it only follows once
        /// the target leaves the window around that height, so ordinary jumps don't move the view.
        /// </summary>
        private void UpdateAnchor()
        {
            float targetY = target.position.y;

            if (targetGround.IsGrounded)
            {
                _anchorY = targetY;
            }
            else if (targetY > _anchorY + maxRiseBeforeFollow)
            {
                _anchorY = targetY - maxRiseBeforeFollow;
            }
            else if (targetY < _anchorY - maxFallBeforeFollow)
            {
                _anchorY = targetY + maxFallBeforeFollow;
            }
        }

        private Vector2 GoalPosition()
        {
            return new Vector2(target.position.x, _anchorY) + framingOffset;
        }

        private Vector2 KeepTargetInView(Vector2 cameraPosition)
        {
            Vector2 reach = Vector2.Max(HalfViewSize() - new Vector2(minEdgeMargin, minEdgeMargin), Vector2.zero);
            Vector2 targetPosition = target.position;

            return new Vector2(
                Mathf.Clamp(cameraPosition.x, targetPosition.x - reach.x, targetPosition.x + reach.x),
                Mathf.Clamp(cameraPosition.y, targetPosition.y - reach.y, targetPosition.y + reach.y));
        }

        private Vector2 ClampToBounds(Vector2 cameraPosition)
        {
            if (bounds == null) return cameraPosition;

            Rect area = bounds.WorldRect;
            Vector2 halfView = HalfViewSize();

            return new Vector2(
                ClampAxis(cameraPosition.x, area.xMin + halfView.x, area.xMax - halfView.x),
                ClampAxis(cameraPosition.y, area.yMin + halfView.y, area.yMax - halfView.y));
        }

        // Read every frame so a window resize or zoom change is picked up immediately.
        private Vector2 HalfViewSize()
        {
            float halfHeight = _camera.orthographicSize;
            return new Vector2(halfHeight * _camera.aspect, halfHeight);
        }

        // An area narrower than the view on an axis has no valid range, so centre on it: clamping to an
        // inverted range would flip the camera between the two edges.
        private static float ClampAxis(float value, float min, float max)
        {
            return min > max ? (min + max) * 0.5f : Mathf.Clamp(value, min, max);
        }

        private void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}
