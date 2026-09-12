using UnityEngine;

namespace SkySteps.Level
{
    /// <summary>
    /// A single pick-up. Adds its points once, then removes itself, which also clears it from the
    /// minimap because the marker disappears with the object.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Coin : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;

        [Tooltip("Tag that counts as collecting the coin.")]
        [SerializeField] private string playerTag = "Player";

        private bool _collected;

        private void Awake()
        {
            if (scoreSystem == null)
            {
                Debug.LogError($"{nameof(Coin)}: no {nameof(ScoreSystem)} assigned.", this);
                enabled = false;
                return;
            }

            scoreSystem.RegisterCoin();
        }

        private void Reset()
        {
            // A coin is something to walk through, never something to stand on.
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag(playerTag)) return;

            _collected = true;
            scoreSystem.CollectCoin();
            Destroy(gameObject);
        }
    }
}
