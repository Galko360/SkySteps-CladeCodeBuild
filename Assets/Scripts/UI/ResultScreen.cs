using System;
using SkySteps.Level;
using SkySteps.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkySteps.UI
{
    /// <summary>
    /// The end-of-run popup, for winning and for losing. One popup with swapped wording rather than
    /// two near-identical ones. Freezes the game while showing, and reloads the level on restart.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResultScreen : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject panel;
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text summaryLabel;

        [Header("Sources")]
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private LevelTimer levelTimer;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Wording")]
        [SerializeField] private string winTitle = "You Win!";
        [SerializeField] private string loseTitle = "Game Over";
        [SerializeField] private Color winColor = Color.white;
        [SerializeField] private Color loseColor = new Color(1f, 0.55f, 0.5f, 1f);

        /// <summary>Raised once when the popup appears; true for a win, false for game over.</summary>
        public event Action<bool> Shown;

        public bool IsShowing => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (panel == null || restartButton == null)
            {
                Debug.LogError($"{nameof(ResultScreen)}: panel and restart button must both be assigned.", this);
                enabled = false;
                return;
            }

            panel.SetActive(false);
            restartButton.onClick.AddListener(Restart);
        }

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.Died += ShowGameOver;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.Died -= ShowGameOver;
        }

        private void OnDestroy()
        {
            if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        }

        /// <summary>Shows the winning result.</summary>
        public void ShowWin()
        {
            Show(winTitle, winColor, won: true);
        }

        /// <summary>Shows the losing result. Also used when the last life is lost.</summary>
        public void ShowGameOver()
        {
            Show(loseTitle, loseColor, won: false);
        }

        private void Show(string title, Color titleColor, bool won)
        {
            if (IsShowing) return;

            if (levelTimer != null) levelTimer.StopTimer();

            if (titleLabel != null)
            {
                titleLabel.text = title;
                titleLabel.color = titleColor;
            }

            if (summaryLabel != null && scoreSystem != null)
            {
                string summary = $"Coins {scoreSystem.CoinsCollected}/{scoreSystem.TotalCoins}   Score {scoreSystem.Score}";
                if (levelTimer != null) summary += $"\nTime {levelTimer.FormatElapsed()}";

                summaryLabel.text = summary;
            }

            panel.SetActive(true);

            // Freezing time stops physics and FixedUpdate without disabling the player's components.
            // The popup still responds, because uGUI input runs on unscaled time.
            Time.timeScale = 0f;

            Shown?.Invoke(won);
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
