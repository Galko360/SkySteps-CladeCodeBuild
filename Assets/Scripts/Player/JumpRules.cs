using System;
using UnityEngine;

namespace SkySteps.Player
{
    /// <summary>What a jump press turned into on a physics step.</summary>
    public enum JumpKind
    {
        None,
        Ground,
        Air
    }

    /// <summary>
    /// Decides when a jump press becomes a jump, and which kind. Owns the forgiveness windows
    /// (coyote time and the jump buffer) and the air-jump budget, so the controller only acts on
    /// the verdict. Plain C# rather than a component: it has no scene presence or lifecycle.
    /// </summary>
    public sealed class JumpRules
    {
        private readonly PlayerMovementSettings _settings;

        private float _coyoteTimer;
        private float _bufferTimer;

        public JumpRules(PlayerMovementSettings settings)
        {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public int AirJumpsRemaining { get; private set; }

        /// <summary>Remembers a jump press for the length of the buffer window.</summary>
        public void RegisterPress()
        {
            _bufferTimer = _settings.JumpBufferTime;
        }

        /// <summary>Advances the windows. Call once per physics step, before <see cref="TryConsume"/>.</summary>
        public void Tick(bool grounded, float deltaTime)
        {
            if (grounded)
            {
                // Grounded only means standing on top of a floor (see PlayerContactProbe), so passing
                // up through a one-way platform cannot refill these.
                _coyoteTimer = _settings.CoyoteTime;
                AirJumpsRemaining = _settings.AirJumps;
            }
            else
            {
                _coyoteTimer = Mathf.Max(0f, _coyoteTimer - deltaTime);
            }

            _bufferTimer = Mathf.Max(0f, _bufferTimer - deltaTime);
        }

        /// <summary>
        /// Spends the buffered press if a jump is allowed right now. A ground jump, including one inside
        /// the coyote window, is always preferred so it never costs an air jump. When neither is allowed
        /// the press stays buffered, so pressing just before landing still jumps on touchdown.
        /// </summary>
        public JumpKind TryConsume()
        {
            if (_bufferTimer <= 0f) return JumpKind.None;

            if (_coyoteTimer > 0f)
            {
                _bufferTimer = 0f;
                _coyoteTimer = 0f;
                return JumpKind.Ground;
            }

            if (AirJumpsRemaining > 0)
            {
                AirJumpsRemaining--;
                _bufferTimer = 0f;
                return JumpKind.Air;
            }

            return JumpKind.None;
        }
    }
}
