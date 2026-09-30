using UnityEngine;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Loops the goal page: the character hops up a flight of ledges, collecting coins on the way,
    /// towards the pulsing goal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TutorialClimbGoalDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform character;
        [SerializeField] private Image characterImage;
        [SerializeField] private RectTransform goal;

        [Tooltip("Coins in the order they are picked up; each disappears as the character reaches it.")]
        [SerializeField] private GameObject[] coins;
        [Tooltip("Fraction of the cycle at which each coin is collected, in the same order.")]
        [SerializeField] private float[] coinPickupTimes;

        [Header("Frames")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite jumpSprite;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float cycleSeconds = 5f;
        [SerializeField] private AnimationCurve horizontal = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [SerializeField] private AnimationCurve vertical = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [Tooltip("How far the character is off a ledge before the pose switches to the jump frame.")]
        [SerializeField] private float airborneThreshold = 12f;
        [SerializeField, Min(1f)] private float idleFramesPerSecond = 6f;
        [SerializeField] private float goalPulse = 0.08f;

        private float _lastY;

        private void Update()
        {
            if (character == null) return;

            float t = Mathf.Repeat(Time.unscaledTime, cycleSeconds) / cycleSeconds;
            float x = horizontal.Evaluate(t);
            float y = vertical.Evaluate(t);
            character.anchoredPosition = new Vector2(x, y);

            if (characterImage != null)
            {
                bool climbing = Mathf.Abs(y - _lastY) > airborneThreshold * Time.unscaledDeltaTime;
                if (climbing) characterImage.sprite = jumpSprite;
                else if (idleFrames != null && idleFrames.Length > 0)
                    characterImage.sprite = idleFrames[(int)(Time.unscaledTime * idleFramesPerSecond) % idleFrames.Length];
            }

            _lastY = y;

            if (coins != null && coinPickupTimes != null)
            {
                for (int i = 0; i < coins.Length && i < coinPickupTimes.Length; i++)
                {
                    if (coins[i] == null) continue;

                    bool uncollected = t < coinPickupTimes[i];
                    if (coins[i].activeSelf != uncollected) coins[i].SetActive(uncollected);
                }
            }

            if (goal != null)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 3f) * goalPulse;
                goal.localScale = new Vector3(pulse, pulse, 1f);
            }
        }
    }
}
