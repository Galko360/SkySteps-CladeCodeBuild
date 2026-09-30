using UnityEngine;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Loops the platform page: the character jumps up through a plank, stands on it, then drops back
    /// down through it. The key hint appears only while the drop is happening.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TutorialDropThroughDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform character;
        [SerializeField] private Image characterImage;
        [SerializeField] private GameObject dropHint;

        [Header("Frames")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite jumpSprite;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float cycleSeconds = 4f;
        [Tooltip("The character's height over the cycle: up through the plank, a pause on top, then down through it.")]
        [SerializeField] private AnimationCurve height = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [Tooltip("The part of the cycle where the character is standing on the plank.")]
        [SerializeField, Range(0f, 1f)] private float restFrom = 0.32f;
        [SerializeField, Range(0f, 1f)] private float restTo = 0.55f;
        [Tooltip("The part of the cycle where the drop-through hint shows.")]
        [SerializeField, Range(0f, 1f)] private float hintFrom = 0.42f;
        [SerializeField, Range(0f, 1f)] private float hintTo = 0.72f;
        [SerializeField, Min(1f)] private float idleFramesPerSecond = 6f;

        private void Update()
        {
            if (character == null) return;

            float t = Mathf.Repeat(Time.unscaledTime, cycleSeconds) / cycleSeconds;
            character.anchoredPosition = new Vector2(character.anchoredPosition.x, height.Evaluate(t));

            bool resting = t >= restFrom && t <= restTo;
            if (characterImage != null)
            {
                if (!resting) characterImage.sprite = jumpSprite;
                else if (idleFrames != null && idleFrames.Length > 0)
                    characterImage.sprite = idleFrames[(int)(Time.unscaledTime * idleFramesPerSecond) % idleFrames.Length];
            }

            if (dropHint != null)
            {
                bool showHint = t >= hintFrom && t <= hintTo;
                if (dropHint.activeSelf != showHint) dropHint.SetActive(showHint);
            }
        }
    }
}
