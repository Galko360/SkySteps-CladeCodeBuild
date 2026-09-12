using System.Collections.Generic;
using SkySteps.Player;
using UnityEngine;

namespace SkySteps.UI
{
    /// <summary>
    /// Shows the remaining lives as a row of hearts. Builds one heart per life at start, then only
    /// changes their colour, so nothing is created or destroyed during play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LivesHud : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private RectTransform container;
        [SerializeField] private Sprite heartSprite;

        [Header("Appearance")]
        [SerializeField] private Color fullColor = Color.white;
        [SerializeField] private Color spentColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField, Min(1f)] private float heartSize = 44f;
        [SerializeField, Min(0f)] private float spacing = 8f;

        private readonly List<UnityEngine.UI.Image> _hearts = new List<UnityEngine.UI.Image>();

        private void Awake()
        {
            if (health == null || container == null || heartSprite == null)
            {
                Debug.LogError($"{nameof(LivesHud)}: health, container and heart sprite must all be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (health != null) health.Changed += Refresh;
        }

        private void OnDisable()
        {
            if (health != null) health.Changed -= Refresh;
        }

        private void Start()
        {
            Build();
            Refresh();
        }

        private void Build()
        {
            for (int i = 0; i < health.MaxLives; i++)
            {
                var go = new GameObject("Heart" + (i + 1), typeof(RectTransform));
                var rect = go.GetComponent<RectTransform>();
                rect.SetParent(container, false);
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(heartSize, heartSize);
                rect.anchoredPosition = new Vector2(i * (heartSize + spacing), 0f);

                var image = go.AddComponent<UnityEngine.UI.Image>();
                image.sprite = heartSprite;
                image.raycastTarget = false;
                _hearts.Add(image);
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                _hearts[i].color = i < health.Lives ? fullColor : spentColor;
            }
        }
    }
}
