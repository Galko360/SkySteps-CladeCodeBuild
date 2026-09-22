using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Effects
{
    /// <summary>
    /// Tints the player red when a hit costs a life, fading back to normal. Only non-fatal hits flash:
    /// the final hit freezes the game behind the result popup, which would leave the tint stuck.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDamageFlash : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);

        [Tooltip("Seconds to fade from the flash colour back to normal.")]
        [SerializeField, Min(0.01f)] private float duration = 0.35f;

        private Color _baseColor;
        private float _remaining;

        private void Awake()
        {
            if (health == null || spriteRenderer == null)
            {
                Debug.LogError($"{nameof(PlayerDamageFlash)}: health and sprite renderer must be assigned.", this);
                enabled = false;
                return;
            }

            _baseColor = spriteRenderer.color;
        }

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

        private void Update()
        {
            if (_remaining <= 0f) return;

            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);

            // Colour only: the invulnerability blink toggles the renderer on and off, so the two compose.
            spriteRenderer.color = Color.Lerp(_baseColor, flashColor, _remaining / duration);
        }
    }
}
