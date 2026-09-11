using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Swept-AABB contact detection. Unity's solver resolves collisions; this reports the contact
    /// <em>state</em> the solver does not expose: grounded, ceiling, and which side a wall is on.
    /// Each probe sweeps the player's own box collider, so results follow the collider
    /// automatically if it is ever resized.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerContactProbe : MonoBehaviour
    {
        private const int MaxHitsPerProbe = 8;

        [Tooltip("Layers treated as solid geometry for contact tests.")]
        [SerializeField] private LayerMask solidLayers;

        [Tooltip("Sweep distance. Long enough to detect resting contact, short enough not to trigger early.")]
        [SerializeField, Min(0.001f)] private float skinWidth = 0.05f;

        [Tooltip("Minimum surface alignment for a hit to count. 0.5 accepts slopes up to roughly 60 degrees.")]
        [SerializeField, Range(0.1f, 1f)] private float minSurfaceAlignment = 0.5f;

        [Tooltip("Fastest the player may move away from a floor and still count as standing on it. " +
                 "Stops a jump being refunded as the player rises past a platform's top edge.")]
        [SerializeField, Min(0f)] private float maxSeparationSpeed = 0.1f;

        // Reused across every cast so contact probing never allocates.
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[MaxHitsPerProbe];
        private ContactFilter2D _filter;
        private Collider2D _collider;
        private Rigidbody2D _body;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingCeiling { get; private set; }

        /// <summary>-1 when touching a wall on the left, +1 on the right, 0 when clear.</summary>
        public int WallDirection { get; private set; }

        /// <summary>The surface currently supporting the player, or null when airborne.</summary>
        public Collider2D GroundCollider { get; private set; }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _body = _collider.attachedRigidbody;
            _filter = new ContactFilter2D { useTriggers = false };
            _filter.SetLayerMask(solidLayers);
        }

        /// <summary>
        /// Refreshes all contact state. Call once per physics step, before movement is applied.
        /// </summary>
        public void Probe()
        {
            IsGrounded = TryCast(Vector2.down, isGroundProbe: true, out RaycastHit2D groundHit);
            GroundCollider = IsGrounded ? groundHit.collider : null;

            IsTouchingCeiling = TryCast(Vector2.up, isGroundProbe: false, out _);

            if (TryCast(Vector2.right, isGroundProbe: false, out _)) WallDirection = 1;
            else if (TryCast(Vector2.left, isGroundProbe: false, out _)) WallDirection = -1;
            else WallDirection = 0;
        }

        private bool TryCast(Vector2 direction, bool isGroundProbe, out RaycastHit2D result)
        {
            int count = _collider.Cast(direction, _filter, _hits, skinWidth);
            Vector2 expectedNormal = -direction;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (hit.collider == null) continue;

                // Rejects glancing contacts and slopes too steep to count as this kind of surface.
                if (Vector2.Dot(hit.normal, expectedNormal) < minSurfaceAlignment) continue;

                if (isGroundProbe)
                {
                    if (!IsStandingOn(hit)) continue;
                }
                else if (hit.collider.usedByEffector)
                {
                    // The solver lets the player pass through one-way platforms from below and from
                    // the side, so treating them as ceiling or wall would cancel motion that succeeds.
                    continue;
                }

                result = hit;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// True when the player is resting or landing on the hit surface: not inside it, and not
        /// moving away from it. Jump refunds depend on this, so it has to mean "on top of".
        /// </summary>
        private bool IsStandingOn(RaycastHit2D hit)
        {
            // A zero-distance hit means the cast started inside the surface. Only one-way platforms
            // can be entered, and only while passing through them, so that is never a floor. Unity
            // reports these hits with a normal pointing straight back along the cast, which is why
            // the alignment test alone accepted them.
            if (hit.collider.usedByEffector && hit.distance <= 0f) return false;

            Vector2 ownVelocity = _body != null ? _body.linearVelocity : Vector2.zero;
            Vector2 surfaceVelocity = hit.rigidbody != null
                ? hit.rigidbody.GetPointVelocity(hit.point)
                : Vector2.zero;

            return Vector2.Dot(ownVelocity - surfaceVelocity, hit.normal) <= maxSeparationSpeed;
        }
    }
}
