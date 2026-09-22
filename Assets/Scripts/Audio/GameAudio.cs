using SkySteps.Level;
using SkySteps.Player;
using SkySteps.UI;
using UnityEngine;

namespace SkySteps.Audio
{
    /// <summary>
    /// Plays the game's sound effects in response to what the gameplay systems report. Gameplay code
    /// only raises events and never touches audio, so sounds can change without touching movement,
    /// scoring or health.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class GameAudio : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private PlayerController player;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ResultScreen resultScreen;

        [Header("Clips")]
        [SerializeField] private AudioClip jump;
        [SerializeField] private AudioClip airJump;
        [SerializeField] private AudioClip land;
        [SerializeField] private AudioClip coin;
        [SerializeField] private AudioClip hurt;
        [SerializeField] private AudioClip win;
        [SerializeField] private AudioClip gameOver;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (player != null)
            {
                player.Jumped += OnJumped;
                player.Landed += OnLanded;
            }

            if (scoreSystem != null) scoreSystem.CoinCollected += OnCoinCollected;
            if (playerHealth != null) playerHealth.Damaged += OnDamaged;
            if (resultScreen != null) resultScreen.Shown += OnResultShown;
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.Jumped -= OnJumped;
                player.Landed -= OnLanded;
            }

            if (scoreSystem != null) scoreSystem.CoinCollected -= OnCoinCollected;
            if (playerHealth != null) playerHealth.Damaged -= OnDamaged;
            if (resultScreen != null) resultScreen.Shown -= OnResultShown;
        }

        private void OnJumped(JumpKind kind) => Play(kind == JumpKind.Air ? airJump : jump);
        private void OnLanded() => Play(land);
        private void OnCoinCollected() => Play(coin);
        private void OnDamaged() => Play(hurt);
        private void OnResultShown(bool won) => Play(won ? win : gameOver);

        // One-shots overlap freely, and play on real time, so the result sounds still play while the
        // popup has the game frozen.
        private void Play(AudioClip clip)
        {
            if (clip != null) _source.PlayOneShot(clip);
        }
    }
}
