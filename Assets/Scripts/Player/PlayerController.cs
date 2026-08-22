using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Orchestrates the player systems: samples intent, tracks the coyote and jump-buffer windows,
    /// and decides each physics step whether a jump press means jump or drop through.
    /// Deliberately holds no movement or collision logic of its own.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerContactProbe contactProbe;
        [SerializeField] private OneWayPlatformDropper dropper;
        [SerializeField] private PlayerMovementSettings settings;

        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private void Awake()
        {
            if (input == null || motor == null || contactProbe == null || dropper == null || settings == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: one or more references are unassigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            // Sampled here rather than in FixedUpdate: a physics step may run zero or several times
            // per frame, so a press polled there can be missed entirely or counted twice.
            if (input.JumpPressedThisFrame)
            {
                _jumpBufferTimer = settings.JumpBufferTime;
            }
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            contactProbe.Probe();
            bool grounded = contactProbe.IsGrounded && !dropper.IsDropping;

            _coyoteTimer = grounded ? settings.CoyoteTime : Mathf.Max(0f, _coyoteTimer - deltaTime);
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);

            motor.ApplyHorizontal(input.MoveX, grounded);
            TryConsumeJump();
            motor.ApplyVerticalModifiers(input.JumpHeld, contactProbe.IsTouchingCeiling);
        }

        private void TryConsumeJump()
        {
            if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f) return;

            // Holding down converts the jump into a drop when the supporting surface allows it.
            bool dropped = input.DownHeld && dropper.TryStartDrop();
            if (!dropped)
            {
                motor.Jump();
            }

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }
    }
}
