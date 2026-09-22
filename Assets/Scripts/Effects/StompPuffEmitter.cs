using SkySteps.Level;
using UnityEngine;

namespace SkySteps.Effects
{
    /// <summary>
    /// Emits a burst of goo where an enemy is stomped.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StompPuffEmitter : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private ParticleSystem puff;
        [SerializeField, Min(1)] private int particlesPerStomp = 12;

        private void Awake()
        {
            if (scoreSystem == null || puff == null)
            {
                Debug.LogError($"{nameof(StompPuffEmitter)}: score system and particle system must be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (scoreSystem != null) scoreSystem.EnemyStomped += OnEnemyStomped;
        }

        private void OnDisable()
        {
            if (scoreSystem != null) scoreSystem.EnemyStomped -= OnEnemyStomped;
        }

        private void OnEnemyStomped(Vector2 position)
        {
            puff.transform.position = position;
            puff.Emit(particlesPerStomp);
        }
    }
}
