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

        private JumpRules _jumpRules;

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

            motor.ApplyHorizontal(input.MoveX, grounded);
            ExecuteJump(_jumpRules.TryConsume());
            motor.ApplyVerticalModifiers(input.JumpHeld, contactProbe.IsTouchingCeiling);
        }

        private void ExecuteJump(JumpKind kind)
        {
            switch (kind)
            {
                case JumpKind.Ground:
                    // Holding down turns a ground jump into a drop when the surface allows it.
                    if (input.DownHeld && dropper.TryStartDrop()) return;
                    motor.Jump(settings.JumpHeight);
                    break;

                case JumpKind.Air:
                    motor.Jump(settings.AirJumpHeight);
                    break;
            }
        }
    }
}
