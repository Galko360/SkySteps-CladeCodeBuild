using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Loops the enemy page: the character drops onto a hopping slime, which squashes flat as the
    /// character bounces off it and the score reward pops up.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TutorialStompDemo : MonoBehaviour
    {
        [SerializeField] private RectTransform character;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image slimeImage;
        [SerializeField] private RectTransform scorePop;
        [SerializeField] private TMP_Text scoreLabel;

        [Header("Frames")]
        [SerializeField] private Sprite fallSprite;
        [SerializeField] private Sprite jumpSprite;
        [SerializeField] private Sprite[] slimeFrames;
        [SerializeField] private Sprite slimeSquashed;

        [Header("Motion")]
        [SerializeField, Min(0.1f)] private float cycleSeconds = 3f;
        [Tooltip("The character's height over the cycle. It touches the slime where this reaches zero.")]
        [SerializeField] private AnimationCurve height = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [Tooltip("Fraction of the cycle at which the stomp lands.")]
        [SerializeField, Range(0f, 1f)] private float stompAt = 0.4f;
        [SerializeField, Min(1f)] private float slimeFramesPerSecond = 8f;
        [SerializeField] private float scorePopRise = 90f;
        [SerializeField, Min(0.1f)] private float scorePopSeconds = 0.9f;

        private void OnEnable()
        {
            if (scorePop != null) scorePop.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (character == null) return;

            float t = Mathf.Repeat(Time.unscaledTime, cycleSeconds) / cycleSeconds;
            float y = height.Evaluate(t);
            character.anchoredPosition = new Vector2(character.anchoredPosition.x, y);

            bool squashed = t >= stompAt;
            if (characterImage != null) characterImage.sprite = squashed ? jumpSprite : fallSprite;

            if (slimeImage != null)
            {
                if (squashed) slimeImage.sprite = slimeSquashed;
                else if (slimeFrames != null && slimeFrames.Length > 0)
                    slimeImage.sprite = slimeFrames[(int)(Time.unscaledTime * slimeFramesPerSecond) % slimeFrames.Length];
            }

            UpdateScorePop(t);
        }

        private void UpdateScorePop(float t)
        {
            if (scorePop == null) return;

            float since = (t - stompAt) * cycleSeconds;
            bool visible = since >= 0f && since <= scorePopSeconds;
            if (scorePop.gameObject.activeSelf != visible) scorePop.gameObject.SetActive(visible);
            if (!visible) return;

            float progress = since / scorePopSeconds;
            scorePop.anchoredPosition = new Vector2(scorePop.anchoredPosition.x, progress * scorePopRise);
            if (scoreLabel != null)
            {
                Color c = scoreLabel.color;
                c.a = 1f - progress * progress;
                scoreLabel.color = c;
            }
        }
    }
}
