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
            winScreen.Show();
        }
    }
}
