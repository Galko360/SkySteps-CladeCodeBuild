using System;
using UnityEngine;

namespace SkySteps.Level
{
    /// <summary>
    /// Keeps the run's score and coin count and announces changes, so the HUD and the win screen
    /// never have to poll it. Coins register themselves, so the total is always the number of coins
    /// actually in the level rather than a figure that can drift out of date.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScoreSystem : MonoBehaviour
    {
        [SerializeField, Min(0)] private int pointsPerCoin = 10;

        /// <summary>Raised whenever the score or the coin count changes.</summary>
        public event Action Changed;

        /// <summary>
        /// Raised when a coin is picked up. Separate from <see cref="Changed"/>, which also fires as
        /// coins register at the start and so cannot tell a pickup apart.
        /// </summary>
        public event Action CoinCollected;

        public int Score { get; private set; }
        public int CoinsCollected { get; private set; }
        public int TotalCoins { get; private set; }
        public int PointsPerCoin => pointsPerCoin;

        /// <summary>Counts a coin that exists in the level. Called by each coin as it wakes up.</summary>
        public void RegisterCoin()
        {
            TotalCoins++;
            Changed?.Invoke();
        }

        /// <summary>Awards a collected coin.</summary>
        public void CollectCoin()
        {
            CoinsCollected++;
            Score += pointsPerCoin;
            Changed?.Invoke();
            CoinCollected?.Invoke();
        }
    }
}
