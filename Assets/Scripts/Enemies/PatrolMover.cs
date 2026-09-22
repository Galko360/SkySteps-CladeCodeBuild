using UnityEngine;

namespace SkySteps.Enemies
{
    /// <summary>
    /// Walks a kinematic body back and forth between two points either side of where it starts,
    /// turning to face the way it is going. Moves on physics time, so pausing freezes it too.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PatrolMover : MonoBehaviour
    {
        [Tooltip("How far either side of the starting point the patrol reaches.")]
        [SerializeField, Min(0f)] private float patrolHalfWidth = 1.5f;
        [SerializeField, Min(0f)] private float speed = 1.6f;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Whether the sprite art is drawn facing right.")]
        [SerializeField] private bool artFacesRight = true;

        private Rigidbody2D _body;
        private float _leftX;
        private float _rightX;
        private int _direction = 1;
        private bool _stopped;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();

            float startX = _body.position.x;
            _leftX = startX - patrolHalfWidth;
            _rightX = startX + patrolHalfWidth;
            Face(_direction);
        }

        private void FixedUpdate()
        {
            if (_stopped) return;

            float x = _body.position.x + _direction * speed * Time.fixedDeltaTime;
            if (x >= _rightX)
            {
                x = _rightX;
                Face(-1);
            }
            else if (x <= _leftX)
            {
                x = _leftX;
                Face(1);
            }

            _body.MovePosition(new Vector2(x, _body.position.y));
        }

        /// <summary>Halts the patrol for good, e.g. once the enemy is defeated.</summary>
        public void Stop()
        {
            _stopped = true;
            _body.linearVelocity = Vector2.zero;
        }

        private void Face(int direction)
        {
            _direction = direction;
            if (spriteRenderer != null) spriteRenderer.flipX = (direction > 0) != artFacesRight;
        }

        private void OnDrawGizmosSelected()
        {
            // Before play the patrol is centred on the current position; during play on the start.
            float left = Application.isPlaying ? _leftX : transform.position.x - patrolHalfWidth;
            float right = Application.isPlaying ? _rightX : transform.position.x + patrolHalfWidth;
            float y = transform.position.y;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(left, y), new Vector3(right, y));
            Gizmos.DrawWireSphere(new Vector3(left, y), 0.1f);
            Gizmos.DrawWireSphere(new Vector3(right, y), 0.1f);
        }
    }
}
