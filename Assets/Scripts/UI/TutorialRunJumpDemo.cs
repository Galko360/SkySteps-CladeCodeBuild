using UnityEngine;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Loops the controls page: the character runs along a ledge, jumps, then jumps again in mid-air.
    /// Runs on unscaled time, so it keeps playing while the game is paused behind it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TutorialRunJumpDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform character;
        [SerializeField] private Image characterImage;

        [Header("Frames")]
        [SerializeField] private Sprite[] runFrames;
        [SerializeField] private Sprite jumpSprite;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float cycleSeconds = 3.4f;
        [SerializeField] private float startX = -420f;
        [SerializeField] private float endX = 420f;
        [SerializeField] private float groundY = -90f;
        [Tooltip("Height above the ledge over the cycle: two arcs, the second a mid-air jump.")]
        [SerializeField] private AnimationCurve height = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [SerializeField, Min(1f)] private float runFramesPerSecond = 12f;

        private void Update()
        {
            if (character == null) return;

            float t = Mathf.Repeat(Time.unscaledTime, cycleSeconds) / cycleSeconds;
            float y = height.Evaluate(t);
            character.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), groundY + y);

            if (characterImage == null) return;

            bool airborne = y > 1f;
            if (airborne) characterImage.sprite = jumpSprite;
            else if (runFrames != null && runFrames.Length > 0)
            {
                int frame = (int)(Time.unscaledTime * runFramesPerSecond) % runFrames.Length;
                characterImage.sprite = runFrames[frame];
            }
        }
    }
}
