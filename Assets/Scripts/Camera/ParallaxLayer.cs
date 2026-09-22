using UnityEngine;

namespace SkySteps.Cameras
{
    /// <summary>
    /// Moves a background layer by a fraction of the camera's movement, so it reads as distant.
    /// A factor of 0 leaves the layer fixed in the world; 1 glues it to the camera and it never
    /// appears to move at all.
    ///
    /// A layer that follows the camera is crossed more slowly than the world, so it must be shorter
    /// than the area it backs, or the camera will run off its edge.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;

        [Tooltip("How much of the camera's movement this layer copies. Lower reads as further away.")]
        [SerializeField, Range(0f, 1f)] private float followFactor = 0.3f;

        private Vector3 _layerStart;
        private Vector3 _cameraStart;
        private bool _captured;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                Debug.LogError($"{nameof(ParallaxLayer)}: no camera assigned.", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            // Captured on the first late update rather than in Awake, so it happens after the camera
            // has snapped to the player and the layer's authored alignment still holds.
            if (!_captured)
            {
                _layerStart = transform.position;
                _cameraStart = cameraTransform.position;
                _captured = true;
                return;
            }

            Vector3 travelled = cameraTransform.position - _cameraStart;
            transform.position = new Vector3(
                _layerStart.x + travelled.x * followFactor,
                _layerStart.y + travelled.y * followFactor,
                _layerStart.z);
        }
    }
}
