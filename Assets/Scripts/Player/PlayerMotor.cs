using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Applies movement to the Rigidbody2D. Owns velocity and gravity shaping; knows nothing
    /// about where the input came from or how contact state was determined.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings;

        private Rigidbody2D _body;
        private float _baseGravityScale;

        public Vector2 Velocity => _body.linearVelocity;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _baseGravityScale = _body.gravityScale;

            if (settings == null)
            {
                Debug.LogError($"{nameof(PlayerMotor)}: no {nameof(PlayerMovementSettings)} assigned.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Drives horizontal velocity toward the input target. Acceleration applies while the
        /// player steers and friction applies when input is released, which is what makes
        /// coasting to a stop feel different from actively turning around.
        /// </summary>
        public void ApplyHorizontal(float moveInput, bool grounded)
        {
            float targetX = moveInput * settings.MaxSpeed;
            bool steering = !Mathf.Approximately(moveInput, 0f);

            float rate = steering
                ? (grounded ? settings.GroundAcceleration : settings.AirAcceleration)
                : (grounded ? settings.GroundFriction : settings.AirFriction);

            Vector2 velocity = _body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetX, rate * Time.fixedDeltaTime);
            _body.linearVelocity = velocity;
        }

        /// <summary>Launches the player at exactly the speed needed to reach the configured height.</summary>
        public void Jump()
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * _baseGravityScale);
            float jumpSpeed = Mathf.Sqrt(2f * gravity * settings.JumpHeight);

            Vector2 velocity = _body.linearVelocity;
            velocity.y = jumpSpeed;
            _body.linearVelocity = velocity;
        }

        /// <summary>
        /// Shapes the jump arc: falls are faster than rises, and releasing the button early cuts
        /// the hop short. Also clamps terminal velocity and kills upward motion into a ceiling.
        /// </summary>
        public void ApplyVerticalModifiers(bool jumpHeld, bool touchingCeiling)
        {
            Vector2 velocity = _body.linearVelocity;

            if (touchingCeiling && velocity.y > 0f)
            {
                velocity.y = 0f;
            }

            if (velocity.y < 0f)
            {
                _body.gravityScale = _baseGravityScale * settings.FallGravityMultiplier;
            }
            else if (velocity.y > 0f && !jumpHeld)
            {
                _body.gravityScale = _baseGravityScale * settings.LowJumpGravityMultiplier;
            }
            else
            {
                _body.gravityScale = _baseGravityScale;
            }

            if (velocity.y < -settings.MaxFallSpeed)
            {
                velocity.y = -settings.MaxFallSpeed;
            }

            _body.linearVelocity = velocity;
        }
    }
}
