using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Cameras
{
    /// <summary>
    /// Produces a short, decaying camera shake when a hit costs the player a life. It only computes an
    /// offset; <see cref="CameraFollow2D"/> applies it, so the shaken view still respects the map bounds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageCameraShake : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;

        [Tooltip("Largest offset, in world units, at the start of the shake.")]
        [SerializeField, Min(0f)] private float amplitude = 0.25f;

        [SerializeField, Min(0.01f)] private float duration = 0.3f;

        [Tooltip("How fast the shake moves. Higher is more of a rattle, lower more of a sway.")]
        [SerializeField, Min(1f)] private float frequency = 30f;

        private float _remaining;

        /// <summary>The offset to add to the camera this frame. Zero when not shaking.</summary>
        public Vector2 Offset { get; private set; }

        private void OnEnable()
        {
            if (health != null) health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (health != null) health.Damaged -= OnDamaged;
        }

        private void OnDamaged()
        {
            _remaining = duration;
        }

        // Update, not LateUpdate, so the offset is ready before the camera reads it in its LateUpdate.
        private void Update()
        {
            if (_remaining <= 0f)
            {
                Offset = Vector2.zero;
                return;
            }

            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            float strength = amplitude * (_remaining / duration);

            // Perlin noise rather than random values gives a smooth shake instead of a jitter.
            float t = Time.time * frequency;
            Offset = new Vector2(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f) * (2f * strength);
        }
    }
}
