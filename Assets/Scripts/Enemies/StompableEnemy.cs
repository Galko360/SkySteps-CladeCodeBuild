using SkySteps.Level;
using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Enemies
{
    /// <summary>
    /// Decides what touching this enemy means. Landing on it from above squashes it, bouncing the
    /// player and scoring; any other touch costs the player a life.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class StompableEnemy : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PatrolMover mover;

        [Header("Squash")]
        [SerializeField] private Animator animator;
        [SerializeField] private string squashTrigger = "Squash";
        [Tooltip("How long the squashed enemy stays visible before it is removed.")]
        [SerializeField, Min(0f)] private float squashDuration = 0.4f;

        [Header("Stomp Rule")]
        [Tooltip("Upward speed below which the player counts as coming down onto the enemy.")]
        [SerializeField] private float maxStompVerticalSpeed = 0.5f;

        private Collider2D _collider;
        private bool _defeated;

        // Looked up once per collider rather than on every contact frame.
        private Collider2D _knownCollider;
        private Rigidbody2D _knownBody;
        private PlayerHealth _knownHealth;
        private PlayerController _knownController;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Resolve(other);
        }

        // Also checked while overlapping, so a player whose invulnerability ends while still touching
        // the enemy is hit, as with spikes.
        private void OnTriggerStay2D(Collider2D other)
        {
            Resolve(other);
        }

        private void Resolve(Collider2D other)
        {
            if (_defeated || !other.CompareTag(playerTag)) return;

            if (other != _knownCollider)
            {
                _knownCollider = other;
                _knownBody = other.attachedRigidbody;
                _knownHealth = other.GetComponent<PlayerHealth>();
                _knownController = other.GetComponent<PlayerController>();
            }

            if (IsStomp(other)) Squash();
            else if (_knownHealth != null) _knownHealth.TakeDamage();
        }

        // A stomp is the player coming down with their feet above the enemy's middle. Anything else,
        // walking into it or jumping up into it, is a hit.
        private bool IsStomp(Collider2D player)
        {
            bool descending = _knownBody == null || _knownBody.linearVelocity.y <= maxStompVerticalSpeed;
            bool feetAbove = player.bounds.min.y >= _collider.bounds.center.y;
            return descending && feetAbove;
        }

        private void Squash()
        {
            _defeated = true;
            _collider.enabled = false;

            if (mover != null) mover.Stop();
            if (animator != null) animator.SetTrigger(squashTrigger);
            if (_knownController != null) _knownController.BounceOffEnemy();
            if (scoreSystem != null) scoreSystem.AwardEnemyStomp(transform.position);

            Destroy(gameObject, squashDuration);
        }
    }
}
