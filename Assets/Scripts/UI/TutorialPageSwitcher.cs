using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Shows the tutorial one page at a time, with Back, Next and Close. Opened by whichever menu
    /// owns it; that menu also routes the cancel key, so the key can never close the tutorial and
    /// act on the menu behind it in the same frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TutorialPageSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject[] pages;

        [Header("Controls")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text pageLabel;

        /// <summary>Raised when the tutorial closes, so the menu behind it can take focus back.</summary>
        public event Action Closed;

        private int _pageIndex;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (panel == null || pages == null || pages.Length == 0 ||
                backButton == null || nextButton == null || closeButton == null)
            {
                Debug.LogError($"{nameof(TutorialPageSwitcher)}: panel, pages and all three buttons must be assigned.", this);
                enabled = false;
                return;
            }

            panel.SetActive(false);
            backButton.onClick.AddListener(ShowPrevious);
            nextButton.onClick.AddListener(ShowNext);
            closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (backButton != null) backButton.onClick.RemoveListener(ShowPrevious);
            if (nextButton != null) nextButton.onClick.RemoveListener(ShowNext);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            panel.SetActive(true);
            ShowPage(0);
        }

        public void Close()
        {
            if (!IsOpen) return;

            panel.SetActive(false);
            Closed?.Invoke();
        }

        private void ShowNext() => ShowPage(_pageIndex + 1);
        private void ShowPrevious() => ShowPage(_pageIndex - 1);

        private void ShowPage(int index)
        {
            _pageIndex = Mathf.Clamp(index, 0, pages.Length - 1);

            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null) pages[i].SetActive(i == _pageIndex);
            }

            backButton.interactable = _pageIndex > 0;
            nextButton.interactable = _pageIndex < pages.Length - 1;
            if (pageLabel != null) pageLabel.text = (_pageIndex + 1) + " / " + pages.Length;

            // Keyboard and gamepad focus follows the page, landing on Close once there is no next page.
            if (EventSystem.current != null)
            {
                Button focus = nextButton.interactable ? nextButton : closeButton;
                EventSystem.current.SetSelectedGameObject(focus.gameObject);
            }
        }
    }
}
