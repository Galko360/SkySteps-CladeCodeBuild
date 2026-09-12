using System.Text;
using SkySteps.Level;
using TMPro;
using UnityEngine;

namespace SkySteps.UI
{
    /// <summary>
    /// Draws the running clock. Reuses one text buffer and TextMeshPro's buffer overload, so
    /// redrawing every frame allocates nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TimerHud : MonoBehaviour
    {
        [SerializeField] private LevelTimer timer;
        [SerializeField] private TMP_Text label;

        private readonly StringBuilder _builder = new StringBuilder(12);

        private void Awake()
        {
            if (timer == null || label == null)
            {
                Debug.LogError($"{nameof(TimerHud)}: timer and label must both be assigned.", this);
                enabled = false;
            }
        }

        // Late, so the clock shows the value the timer reached this frame.
        private void LateUpdate()
        {
            _builder.Clear();
            LevelTimer.Append(_builder, timer.Elapsed);
            label.SetText(_builder);
        }
    }
}
