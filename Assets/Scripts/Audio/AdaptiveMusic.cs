using SkySteps.UI;
using UnityEngine;

namespace SkySteps.Audio
{
    /// <summary>
    /// Plays the background music as two synchronised layers: a base track that always plays, and a
    /// space layer that fades in as the player climbs, so the music moves from sky to space along with
    /// the visuals. Stops when the result popup appears, so the fanfare plays alone.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdaptiveMusic : MonoBehaviour
    {
        [SerializeField] private AudioSource baseLayer;
        [SerializeField] private AudioSource spaceLayer;
        [SerializeField] private Transform player;
        [SerializeField] private ResultScreen resultScreen;

        [Header("Space layer")]
        [Tooltip("Height at which the space layer starts to fade in.")]
        [SerializeField] private float fadeInStartY = 12f;

        [Tooltip("Height at which the space layer reaches full volume.")]
        [SerializeField] private float fullVolumeY = 42f;

        [SerializeField, Range(0f, 1f)] private float spaceMaxVolume = 0.6f;

        [Tooltip("Seconds taken to follow a change in height, so a respawn does not jump the mix.")]
        [SerializeField, Min(0.01f)] private float blendTime = 1.5f;

        private float _blend;

        private void Awake()
        {
            if (baseLayer == null || spaceLayer == null || player == null)
            {
                Debug.LogError($"{nameof(AdaptiveMusic)}: both layers and the player must be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (resultScreen != null) resultScreen.Shown += OnResultShown;
        }

        private void OnDisable()
        {
            if (resultScreen != null) resultScreen.Shown -= OnResultShown;
        }

        private void Start()
        {
            // Both layers are scheduled on the same audio-clock time, so they start sample-aligned and,
            // being exactly the same length, loop in step for as long as they play.
            double startAt = AudioSettings.dspTime + 0.1;
            spaceLayer.volume = 0f;
            baseLayer.PlayScheduled(startAt);
            spaceLayer.PlayScheduled(startAt);
        }

        private void Update()
        {
            float target = Mathf.InverseLerp(fadeInStartY, fullVolumeY, player.position.y);
            _blend = Mathf.MoveTowards(_blend, target, Time.unscaledDeltaTime / blendTime);
            spaceLayer.volume = _blend * spaceMaxVolume;
        }

        private void OnResultShown(bool won)
        {
            baseLayer.Stop();
            spaceLayer.Stop();
        }
    }
}
