using System;
using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// The player's lives, damage and respawning. Remembers the last surface the player stood still
    /// on, so a hit sends them back there instead of to the bottom of a long climb.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerContactProbe contactProbe;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Min(1)] private int maxLives = 3;

        [Tooltip("How long the player cannot be hurt again after taking a hit.")]
        [SerializeField, Min(0f)] private float invulnerableTime = 1.5f;

        [Tooltip("How fast the sprite blinks while invulnerable.")]
        [SerializeField, Min(0.01f)] private float blinkInterval = 0.12f;

        /// <summary>Raised whenever the number of lives changes.</summary>
        public event Action Changed;

        /// <summary>Raised once when the last life is lost.</summary>
        public event Action Died;

        /// <summary>
        /// Raised when a hit costs a life but is not the last one. The final hit raises only
        /// <see cref="Died"/>, so listeners never get both for the same blow.
        /// </summary>
        public event Action Damaged;

        public int Lives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsInvulnerable => _invulnerableFor > 0f;

        private Rigidbody2D _body;
        private Vector2 _respawnPoint;
        private float _invulnerableFor;
        private float _blinkTimer;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            Lives = maxLives;
            _respawnPoint = _body.position;

            if (contactProbe == null || spriteRenderer == null)
            {
                Debug.LogError($"{nameof(PlayerHealth)}: contact probe and sprite renderer must be assigned.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            // Resting on a surface marks it as the place to come back to. Requiring near-zero vertical
            // speed avoids saving a spot mid-landing, before the player has actually settled.
            if (contactProbe.IsGrounded && Mathf.Abs(_body.linearVelocity.y) < 0.1f)
            {
                _respawnPoint = _body.position;
            }
        }

        private void Update()
        {
            if (_invulnerableFor <= 0f) return;

            _invulnerableFor -= Time.deltaTime;
            _blinkTimer -= Time.deltaTime;

            if (_blinkTimer <= 0f)
            {
                _blinkTimer = blinkInterval;
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            if (_invulnerableFor <= 0f) spriteRenderer.enabled = true;
        }

        /// <summary>
        /// Costs a life and returns the player to the last safe spot. Does nothing while invulnerable
        /// or once the lives are gone, so one hazard cannot drain several lives at once.
        /// </summary>
        public bool TakeDamage()
        {
            if (IsInvulnerable || Lives <= 0) return false;

            Lives--;
            Changed?.Invoke();

            if (Lives <= 0)
            {
                Died?.Invoke();
                return true;
            }

            Damaged?.Invoke();

            _body.position = _respawnPoint;
            _body.linearVelocity = Vector2.zero;
            _invulnerableFor = invulnerableTime;
            _blinkTimer = blinkInterval;
            return true;
        }
    }
}
