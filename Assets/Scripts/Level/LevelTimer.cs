using System.Text;
using UnityEngine;

namespace SkySteps.Level
{
    /// <summary>
    /// Times the run. Counts in game time, so it freezes along with the game when the win screen
    /// pauses it, and the formatting lives here so the on-screen clock and the result line agree.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelTimer : MonoBehaviour
    {
        public float Elapsed { get; private set; }
        public bool IsRunning { get; private set; } = true;

        /// <summary>Stops the clock, once the level is finished.</summary>
        public void StopTimer()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (IsRunning) Elapsed += Time.deltaTime;
        }

        /// <summary>
        /// Appends a time as MM:SS.mmm. Takes a builder because the on-screen clock redraws every
        /// frame, and building a new string each time would produce constant garbage.
        /// </summary>
        public static void Append(StringBuilder builder, float seconds)
        {
            int totalMilliseconds = Mathf.Max(0, Mathf.FloorToInt(seconds * 1000f));

            AppendPadded(builder, totalMilliseconds / 60000, 2);
            builder.Append(':');
            AppendPadded(builder, totalMilliseconds / 1000 % 60, 2);
            builder.Append('.');
            AppendPadded(builder, totalMilliseconds % 1000, 3);
        }

        /// <summary>The elapsed time as MM:SS.mmm. Allocates, so use it for one-off text only.</summary>
        public string FormatElapsed()
        {
            var builder = new StringBuilder(12);
            Append(builder, Elapsed);
            return builder.ToString();
        }

        private static void AppendPadded(StringBuilder builder, int value, int digits)
        {
            int threshold = 1;
            for (int i = 1; i < digits; i++) threshold *= 10;

            for (; threshold > 1 && value < threshold; threshold /= 10) builder.Append('0');
            builder.Append(value);
        }
    }
}
