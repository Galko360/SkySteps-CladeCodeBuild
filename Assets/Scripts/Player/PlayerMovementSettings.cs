using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>
    /// Tuning data for the player's movement and jump systems. Stored as an asset so feel can be
    /// iterated on, and swapped per character, without touching code or scene state.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlayerMovementSettings",
        menuName = "Sky Steps/Player Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [Header("Horizontal Speed")]
        [SerializeField, Min(0f)] private float maxSpeed = 8f;

        [Header("Acceleration (units/sec squared)")]
        [SerializeField, Min(0f)] private float groundAcceleration = 90f;
        [SerializeField, Min(0f)] private float airAcceleration = 45f;

        [Header("Friction (units/sec squared)")]
        [SerializeField, Min(0f)] private float groundFriction = 70f;
        [SerializeField, Min(0f)] private float airFriction = 12f;

        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpHeight = 3.2f;
        [SerializeField, Min(0f)] private float maxFallSpeed = 22f;

        [Tooltip("Gravity multiplier while falling. Above 1 removes the floaty feel of a symmetric arc.")]
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.9f;

        [Tooltip("Gravity multiplier while rising with jump released, producing variable jump height.")]
        [SerializeField, Min(1f)] private float lowJumpGravityMultiplier = 2.6f;

        [Header("Air Jumps")]
        [Tooltip("Extra jumps available in mid-air: 1 is a double jump. Refilled whenever the player " +
                 "stands on a floor, so walking off a ledge still leaves them available.")]
        [SerializeField, Min(0)] private int airJumps = 1;

        [Tooltip("How high an air jump rises from the point where it starts.")]
        [SerializeField, Min(0f)] private float airJumpHeight = 3.2f;

        [Header("Forgiveness Windows (seconds)")]
        [Tooltip("Grace period after walking off a ledge during which a jump is still accepted.")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;

        [Tooltip("How long a jump press is remembered while airborne, so landing consumes it.")]
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;

        [Header("Drop Through")]
        [Tooltip("How long the player ignores one-way platforms after a drop input.")]
        [SerializeField, Min(0f)] private float dropThroughDuration = 0.35f;

        [Tooltip("Downward speed applied when a drop starts. A body resting on a platform is asleep " +
                 "and will not fall on its own once its support stops colliding, so it needs a nudge.")]
        [SerializeField, Min(0f)] private float dropThroughImpulse = 3f;

        public float MaxSpeed => maxSpeed;
        public float GroundAcceleration => groundAcceleration;
        public float AirAcceleration => airAcceleration;
        public float GroundFriction => groundFriction;
        public float AirFriction => airFriction;
        public float JumpHeight => jumpHeight;
        public float MaxFallSpeed => maxFallSpeed;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;
        public int AirJumps => airJumps;
        public float AirJumpHeight => airJumpHeight;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float DropThroughDuration => dropThroughDuration;
        public float DropThroughImpulse => dropThroughImpulse;
    }
}
