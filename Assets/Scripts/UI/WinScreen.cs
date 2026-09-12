using SkySteps.Level;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkySteps.UI
{
    /// <summary>
    /// The "You Win!" popup. Freezes the game while it is showing, and reloads the level on restart.
    /// Lives on an always-active object so it can switch the popup itself on and off.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WinScreen : MonoBehaviour
    {
        [Tooltip("Root object of the popup. Hidden until the level is completed.")]
        [SerializeField] private GameObject panel;

        [SerializeField] private UnityEngine.UI.Button restartButton;

        [Header("Result")]
        [Tooltip("Optional. With both set, the popup also shows the final coins and score.")]
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private TMP_Text summaryLabel;

        public bool IsShowing => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (panel == null || restartButton == null)
            {
                Debug.LogError($"{nameof(WinScreen)}: panel and restart button must both be assigned.", this);
                enabled = false;
                return;
            }

            panel.SetActive(false);
            restartButton.onClick.AddListener(Restart);
        }

        private void OnDestroy()
        {
            if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        }

        /// <summary>Shows the popup and freezes gameplay. Safe to call more than once.</summary>
        public void Show()
        {
            if (summaryLabel != null && scoreSystem != null)
            {
                summaryLabel.text =
                    $"Coins {scoreSystem.CoinsCollected}/{scoreSystem.TotalCoins}   Score {scoreSystem.Score}";
            }

            panel.SetActive(true);

            // Freezing time stops physics and FixedUpdate without disabling the player's components.
            // The popup still responds, because uGUI input runs on unscaled time.
            Time.timeScale = 0f;
        }

        /// <summary>Reloads the current level from the start.</summary>
        public void Restart()
        {
            // Time has to be restored first, or the reloaded scene starts frozen.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
