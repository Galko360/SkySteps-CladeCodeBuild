using SkySteps.Level;
using UnityEngine;

namespace SkySteps.Effects
{
    /// <summary>
    /// Emits a sparkle burst where a coin is collected. Coins are collected on contact, so the player's
    /// position at that moment is where the coin was.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoinSparkleEmitter : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private Transform player;
        [SerializeField] private ParticleSystem sparkle;
        [SerializeField, Min(1)] private int particlesPerCoin = 14;

        private void Awake()
        {
            if (scoreSystem == null || player == null || sparkle == null)
            {
                Debug.LogError($"{nameof(CoinSparkleEmitter)}: score system, player and particle system must be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (scoreSystem != null) scoreSystem.CoinCollected += OnCoinCollected;
        }

        private void OnDisable()
        {
            if (scoreSystem != null) scoreSystem.CoinCollected -= OnCoinCollected;
        }

        private void OnCoinCollected()
        {
            sparkle.transform.position = player.position;
            sparkle.Emit(particlesPerCoin);
        }
    }
}
