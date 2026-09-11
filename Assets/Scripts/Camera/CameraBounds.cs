using UnityEngine;

namespace SkySteps.Cameras
{
    /// <summary>
    /// The part of the level the camera is allowed to show, centred on this object. Kept in the scene
    /// next to the level geometry, because the level, not the camera, decides where the map ends.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraBounds : MonoBehaviour
    {
        [Tooltip("Width and height of the visible area in world units, centred on this object.")]
        [SerializeField] private Vector2 size = new Vector2(24f, 14f);

        /// <summary>World-space rectangle the camera view must stay inside.</summary>
        public Rect WorldRect
        {
            get
            {
                Vector2 center = transform.position;
                return new Rect(center - size * 0.5f, size);
            }
        }

        private void OnValidate()
        {
            size = Vector2.Max(size, Vector2.zero);
        }

        private void OnDrawGizmos()
        {
            Rect area = WorldRect;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(area.center, area.size);
        }
    }
}
