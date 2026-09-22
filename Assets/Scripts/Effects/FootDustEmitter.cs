using SkySteps.Player;
using UnityEngine;

namespace SkySteps.Effects
{
    /// <summary>
    /// Emits a puff of dust at the player's feet on takeoff and on landing. An air jump gets a smaller
    /// puff, so the double jump reads visually as well as by sound.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FootDustEmitter : MonoBehaviour
    {
        [SerializeField] private PlayerController player;

        [Tooltip("The player's collider. Its bottom edge is where the feet are.")]
        [SerializeField] private Collider2D playerCollider;

        [SerializeField] private ParticleSystem dust;

        [Header("Particles per puff")]
        [SerializeField, Min(1)] private int groundJumpParticles = 6;
        [SerializeField, Min(1)] private int airJumpParticles = 3;
        [SerializeField, Min(1)] private int landingParticles = 10;

        private void Awake()
        {
            if (player == null || playerCollider == null || dust == null)
            {
                Debug.LogError($"{nameof(FootDustEmitter)}: player, collider and particle system must be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (player == null) return;

            player.Jumped += OnJumped;
            player.Landed += OnLanded;
        }

        private void OnDisable()
        {
            if (player == null) return;

            player.Jumped -= OnJumped;
            player.Landed -= OnLanded;
        }

        private void OnJumped(JumpKind kind) => EmitAtFeet(kind == JumpKind.Air ? airJumpParticles : groundJumpParticles);
        private void OnLanded() => EmitAtFeet(landingParticles);

        private void EmitAtFeet(int count)
        {
            // The system simulates in world space, so moving it only affects the new puff.
            Bounds body = playerCollider.bounds;
            dust.transform.position = new Vector3(body.center.x, body.min.y, 0f);
            dust.Emit(count);
        }
    }
}
