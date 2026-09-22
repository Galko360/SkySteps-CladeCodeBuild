using System;
using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Orchestrates the player systems: samples intent, feeds <see cref="JumpRules"/>, and carries
    /// out its verdict each physics step as a jump, an air jump, or a drop through.
    /// Deliberately holds no movement, collision, or jump-eligibility logic of its own.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerContactProbe contactProbe;
        [SerializeField] private OneWayPlatformDropper dropper;
        [SerializeField] private PlayerMovementSettings settings;

        [Tooltip("Slowest fall, in units per second, that counts as a landing rather than stepping down.")]
        [SerializeField, Min(0f)] private float minLandingSpeed = 6f;

        /// <summary>Raised when a jump actually happens, saying whether it was from the ground or mid-air.</summary>
        public event Action<JumpKind> Jumped;

        /// <summary>Raised when the player touches down after a real fall.</summary>
        public event Action Landed;

        private JumpRules _jumpRules;
        private bool _wasGrounded;
        private float _peakFallSpeed;

        private void Awake()
        {
            if (input == null || motor == null || contactProbe == null || dropper == null || settings == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: one or more references are unassigned.", this);
                enabled = false;
                return;
            }

            _jumpRules = new JumpRules(settings);
        }

        private void Update()
        {
            // Sampled here rather than in FixedUpdate: a physics step may run zero or several times
            // per frame, so a press polled there can be missed entirely or counted twice.
            if (input.JumpPressedThisFrame)
            {
                _jumpRules.RegisterPress();
            }
        }

        private void FixedUpdate()
        {
            contactProbe.Probe();
            bool grounded = contactProbe.IsGrounded && !dropper.IsDropping;
            _jumpRules.Tick(grounded, Time.fixedDeltaTime);
            TrackLanding(grounded);

            motor.ApplyHorizontal(input.MoveX, grounded);
            ExecuteJump(_jumpRules.TryConsume());
            motor.ApplyVerticalModifiers(input.JumpHeld, contactProbe.IsTouchingCeiling);
        }

        private void TrackLanding(bool grounded)
        {
            if (!grounded)
            {
                // The fastest fall is remembered while airborne because by the time the probe reports
                // ground, the solver has already stopped the body and its speed reads as zero.
                _peakFallSpeed = Mathf.Max(_peakFallSpeed, -motor.Velocity.y);
            }
            else if (!_wasGrounded && _peakFallSpeed >= minLandingSpeed)
            {
                Landed?.Invoke();
            }

            if (grounded) _peakFallSpeed = 0f;
            _wasGrounded = grounded;
        }

        private void ExecuteJump(JumpKind kind)
        {
            switch (kind)
            {
                case JumpKind.Ground:
                    // Holding down turns a ground jump into a drop when the surface allows it.
                    if (input.DownHeld && dropper.TryStartDrop()) return;
                    motor.Jump(settings.JumpHeight);
                    Jumped?.Invoke(kind);
                    break;

                case JumpKind.Air:
                    motor.Jump(settings.AirJumpHeight);
                    Jumped?.Invoke(kind);
                    break;
            }
        }

        /// <summary>
        /// Launches the player upward off a stomped enemy and gives back the double jump, so a stomp
        /// can be chained into further climbing.
        /// </summary>
        public void BounceOffEnemy()
        {
            if (!enabled) return;

            motor.Jump(settings.StompBounceHeight);
            _jumpRules.RefillAirJumps();
        }
    }
}
