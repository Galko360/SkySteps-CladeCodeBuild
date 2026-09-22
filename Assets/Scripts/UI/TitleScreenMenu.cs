using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// The title screen's buttons: start the level, or quit the game.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TitleScreenMenu : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string levelSceneName = "Level01";

        private void Awake()
        {
            if (playButton == null || quitButton == null)
            {
                Debug.LogError($"{nameof(TitleScreenMenu)}: play and quit buttons must both be assigned.", this);
                enabled = false;
                return;
            }

            playButton.onClick.AddListener(Play);
            quitButton.onClick.AddListener(Quit);
        }

        private void Start()
        {
            // Gives keyboard and gamepad players a focused button without touching the mouse.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(Play);
            if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
        }

        public void Play()
        {
            SceneLoader.Load(levelSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
