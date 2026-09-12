using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Drives the character's animator from the movement systems that already exist: horizontal
    /// speed, grounded state and vertical speed. Holds no movement logic of its own, so animation
    /// can never disagree with what the player is actually doing.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerContactProbe contactProbe;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Horizontal speed above which the character counts as running rather than standing.")]
        [SerializeField, Min(0f)] private float runThreshold = 0.3f;

        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int GroundedId = Animator.StringToHash("Grounded");
        private static readonly int VerticalSpeedId = Animator.StringToHash("VerticalSpeed");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (contactProbe == null || body == null || spriteRenderer == null)
            {
                Debug.LogError($"{nameof(PlayerAnimator)}: probe, body and sprite renderer must all be assigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Vector2 velocity = body.linearVelocity;

            _animator.SetFloat(SpeedId, Mathf.Abs(velocity.x));
            _animator.SetBool(GroundedId, contactProbe.IsGrounded);
            _animator.SetFloat(VerticalSpeedId, velocity.y);

            // Face the way we are moving, and keep the last facing when standing still.
            if (Mathf.Abs(velocity.x) > runThreshold) spriteRenderer.flipX = velocity.x < 0f;
        }
    }
}
