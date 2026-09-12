using SkySteps.Level;
using TMPro;
using UnityEngine;

namespace SkySteps.UI
{
    /// <summary>Shows the live coin count and score on screen.</summary>
    [DisallowMultipleComponent]
    public sealed class ScoreHud : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private TMP_Text label;

        private void Awake()
        {
            if (scoreSystem == null || label == null)
            {
                Debug.LogError($"{nameof(ScoreHud)}: score system and label must both be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (scoreSystem == null) return;

            scoreSystem.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (scoreSystem != null) scoreSystem.Changed -= Refresh;
        }

        private void Start()
        {
            // Every coin has registered by now, so the total is final.
            Refresh();
        }

        private void Refresh()
        {
            label.text = $"Coins {scoreSystem.CoinsCollected}/{scoreSystem.TotalCoins}   Score {scoreSystem.Score}";
        }
    }
}
