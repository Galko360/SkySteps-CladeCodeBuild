using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Level
{
    /// <summary>
    /// Anything that costs the player a life on contact: spikes now, enemies later. Deliberately
    /// knows only about damage, so the same component covers every kind of danger.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hazard : MonoBehaviour
    {
        [Tooltip("Tag that can be hurt by this hazard.")]
        [SerializeField] private string playerTag = "Player";

        // Looked up once per collider rather than on every contact frame.
        private Collider2D _knownCollider;
        private PlayerHealth _knownHealth;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        // Also checked while overlapping: a player who respawns onto a hazard, or whose invulnerability
        // runs out while still touching one, would otherwise never be hit.
        private void OnTriggerStay2D(Collider2D other)
        {
            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            if (other != _knownCollider)
            {
                _knownCollider = other;
                _knownHealth = other.GetComponent<PlayerHealth>();
            }

            if (_knownHealth != null) _knownHealth.TakeDamage();
        }
    }
}
