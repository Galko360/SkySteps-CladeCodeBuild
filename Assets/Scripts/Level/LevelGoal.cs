using SkySteps.UI;
using UnityEngine;

namespace SkySteps.Level
{
    /// <summary>
    /// The end of the level. Raises the win screen the first time the player enters the trigger.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelGoal : MonoBehaviour
    {
        [SerializeField] private WinScreen winScreen;

        [Tooltip("Optional. Stopped on arrival, so the popup can show the finish time.")]
        [SerializeField] private LevelTimer levelTimer;

        [Tooltip("Tag that counts as the player reaching the goal.")]
        [SerializeField] private string playerTag = "Player";

        private bool _reached;

        private void Awake()
        {
            if (winScreen == null)
            {
                Debug.LogError($"{nameof(LevelGoal)}: no {nameof(WinScreen)} assigned.", this);
                enabled = false;
            }
        }

        private void Reset()
        {
            // A goal is a volume to walk into, never something to stand on.
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_reached || !other.CompareTag(playerTag)) return;

            _reached = true;

            // Stopped before the popup reads it, so the time shown is the time of arrival.
            if (levelTimer != null) levelTimer.StopTimer();

            winScreen.Show();
        }
    }
}
