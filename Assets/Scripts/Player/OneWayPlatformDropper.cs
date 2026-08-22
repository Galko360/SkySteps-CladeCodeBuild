using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Lets the player fall through the one-way platform they are standing on, by ignoring that
    /// one collider pair for a short window.
    ///
    /// A layer switch looks tidier but does not work: re-assigning gameObject.layer leaves contacts
    /// the solver has already resolved in place, so a player resting on a platform never separates.
    /// Physics2D.IgnoreCollision re-evaluates the specific pair immediately.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class OneWayPlatformDropper : MonoBehaviour
    {
        [SerializeField] private PlayerContactProbe contactProbe;
        [SerializeField] private PlayerMovementSettings settings;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private Collider2D _ignoredPlatform;
        private float _timeRemaining;

        public bool IsDropping => _timeRemaining > 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            if (contactProbe == null || settings == null)
            {
                Debug.LogError(
                    $"{nameof(OneWayPlatformDropper)}: missing contact probe or settings reference.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Starts a drop when standing on a one-way surface. Returns false on solid ground so the
        /// caller can fall back to a normal jump.
        /// </summary>
        public bool TryStartDrop()
        {
            if (IsDropping) return false;

            Collider2D ground = contactProbe.GroundCollider;
            if (ground == null || !ground.usedByEffector) return false;

            Physics2D.IgnoreCollision(_collider, ground, true);
            _ignoredPlatform = ground;
            _timeRemaining = settings.DropThroughDuration;

            // A body resting on a platform is asleep and will not start falling just because its
            // support stopped colliding, so it needs waking and a nudge clear of the surface.
            _body.WakeUp();

            Vector2 velocity = _body.linearVelocity;
            if (velocity.y > -settings.DropThroughImpulse)
            {
                velocity.y = -settings.DropThroughImpulse;
            }

            _body.linearVelocity = velocity;
            return true;
        }

        private void FixedUpdate()
        {
            if (_timeRemaining <= 0f) return;

            _timeRemaining -= Time.fixedDeltaTime;
            if (_timeRemaining <= 0f)
            {
                RestoreCollision();
            }
        }

        /// <summary>Re-enables the ignored pair. Safe to call when no drop is in progress.</summary>
        private void RestoreCollision()
        {
            _timeRemaining = 0f;
            if (_ignoredPlatform == null) return;

            Physics2D.IgnoreCollision(_collider, _ignoredPlatform, false);
            _ignoredPlatform = null;
        }

        private void OnDisable()
        {
            // Never leave a collider pair ignoring each other permanently.
            RestoreCollision();
        }
    }
}
