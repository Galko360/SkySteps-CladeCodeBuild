using System.Collections.Generic;
using SkySteps.Cameras;
using UnityEngine;

namespace SkySteps.UI
{
    /// <summary>
    /// A schematic map of the level, drawn from the level's own geometry: one bar per piece of
    /// terrain, a dot for the player and a marker for the goal. Built once at start, so it follows
    /// level edits without any extra bookkeeping.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Minimap : MonoBehaviour
    {
        [Header("Sources")]
        [Tooltip("The playable area the map represents: the same bounds the camera is clamped to.")]
        [SerializeField] private CameraBounds worldBounds;

        [Tooltip("Parent of the level geometry. Children on the mapped layers become bars.")]
        [SerializeField] private Transform levelRoot;

        [SerializeField] private Transform player;
        [SerializeField] private Transform goal;

        [Header("Map")]
        [Tooltip("Area the map is drawn into. Bars and markers are positioned inside it.")]
        [SerializeField] private RectTransform mapArea;

        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private RectTransform goalMarker;

        [Header("Appearance")]
        [SerializeField] private LayerMask mappedLayers;
        [SerializeField] private Color platformColor = new Color(0.55f, 0.75f, 0.95f, 0.9f);

        [Tooltip("Bars smaller than this many pixels are widened, so thin platforms stay visible.")]
        [SerializeField, Min(1f)] private float minBarSize = 3f;

        private readonly List<RectTransform> _bars = new List<RectTransform>();

        // Parallel lists: a marker and the dot drawn for it. A destroyed marker leaves a null here,
        // which is how a collected coin disappears from the map.
        private readonly List<MinimapMarker> _markers = new List<MinimapMarker>();
        private readonly List<RectTransform> _markerDots = new List<RectTransform>();

        private void Start()
        {
            if (worldBounds == null || levelRoot == null || player == null || mapArea == null || playerMarker == null)
            {
                Debug.LogError($"{nameof(Minimap)}: one or more references are unassigned.", this);
                enabled = false;
                return;
            }

            BuildBars();
            BuildMarkers();

            // Bars are created after the markers exist, so lift the markers back on top.
            if (goalMarker != null)
            {
                goalMarker.SetAsLastSibling();
                if (goal != null) goalMarker.anchoredPosition = ToMap(goal.position);
                else goalMarker.gameObject.SetActive(false);
            }

            playerMarker.SetAsLastSibling();
        }

        private void LateUpdate()
        {
            playerMarker.anchoredPosition = ToMap(player.position);
            UpdateMarkers();
        }

        private void BuildBars()
        {
            Rect world = worldBounds.WorldRect;
            Vector2 mapSize = mapArea.rect.size;

            foreach (Transform child in levelRoot)
            {
                if ((mappedLayers.value & (1 << child.gameObject.layer)) == 0) continue;

                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite == null) continue;

                Bounds worldBox = sprite.bounds;
                RectTransform bar = CreateDot("Bar_" + child.name, platformColor);
                bar.sizeDelta = new Vector2(
                    Mathf.Max(minBarSize, worldBox.size.x / world.width * mapSize.x),
                    Mathf.Max(minBarSize, worldBox.size.y / world.height * mapSize.y));
                bar.anchoredPosition = ToMap(worldBox.center);
                _bars.Add(bar);
            }
        }

        private RectTransform CreateDot(string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(mapArea, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        /// <summary>Draws a dot for every <see cref="MinimapMarker"/> in the level, such as coins.</summary>
        private void BuildMarkers()
        {
            var found = levelRoot.GetComponentsInChildren<MinimapMarker>(true);
            for (int i = 0; i < found.Length; i++)
            {
                MinimapMarker marker = found[i];
                RectTransform dot = CreateDot("Marker_" + marker.name, marker.Color);
                dot.sizeDelta = new Vector2(marker.SizePixels, marker.SizePixels);
                dot.anchoredPosition = ToMap(marker.transform.position);

                _markers.Add(marker);
                _markerDots.Add(dot);
            }
        }

        private void UpdateMarkers()
        {
            for (int i = 0; i < _markers.Count; i++)
            {
                MinimapMarker marker = _markers[i];
                RectTransform dot = _markerDots[i];

                if (marker == null)
                {
                    // Its object is gone, e.g. a collected coin.
                    if (dot.gameObject.activeSelf) dot.gameObject.SetActive(false);
                    continue;
                }

                dot.anchoredPosition = ToMap(marker.transform.position);
            }
        }

        /// <summary>Converts a world position into a position inside the map area.</summary>
        private Vector2 ToMap(Vector3 worldPosition)
        {
            Rect world = worldBounds.WorldRect;
            Vector2 normalized = new Vector2(
                Mathf.InverseLerp(world.xMin, world.xMax, worldPosition.x),
                Mathf.InverseLerp(world.yMin, world.yMax, worldPosition.y));

            return normalized * mapArea.rect.size;
        }
    }
}
