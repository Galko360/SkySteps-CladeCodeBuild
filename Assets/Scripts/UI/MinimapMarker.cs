using UnityEngine;

namespace SkySteps.UI
{
    /// <summary>
    /// Marks this object to be drawn on the <see cref="Minimap"/>. Deliberately knows nothing about
    /// what the object is, so coins, checkpoints or anything else can appear on the map without the
    /// map needing to know about them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MinimapMarker : MonoBehaviour
    {
        [SerializeField] private Color color = Color.yellow;

        [Tooltip("Size of the dot on the map, in pixels.")]
        [SerializeField, Min(1f)] private float sizePixels = 9f;

        public Color Color => color;
        public float SizePixels => sizePixels;
    }
}
