using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// The title screen's buttons: start the level, open the tutorial, or quit the game.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TitleScreenMenu : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string levelSceneName = "Level01";

        [Header("Tutorial")]
        [SerializeField] private TutorialPageSwitcher tutorial;
        [Tooltip("Closes the tutorial. Read here rather than inside the tutorial, so one press cannot " +
                 "both close it and act on the menu behind it.")]
        [SerializeField] private InputActionReference cancelAction;

        private bool _enabledCancel;

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

            if (tutorialButton != null) tutorialButton.onClick.AddListener(OpenTutorial);
        }

        private void OnEnable()
        {
            if (tutorial != null) tutorial.Closed += OnTutorialClosed;

            // Same rule as the player's input: only switch on an action that is off.
            if (cancelAction != null && cancelAction.action != null && !cancelAction.action.enabled)
            {
                cancelAction.action.Enable();
                _enabledCancel = true;
            }
        }

        private void OnDisable()
        {
            if (tutorial != null) tutorial.Closed -= OnTutorialClosed;
            if (_enabledCancel) cancelAction.action.Disable();
            _enabledCancel = false;
        }

        private void Update()
        {
            if (tutorial == null || !tutorial.IsOpen) return;
            if (cancelAction == null || cancelAction.action == null) return;

            if (cancelAction.action.WasPressedThisFrame()) tutorial.Close();
        }

        private void OpenTutorial()
        {
            if (tutorial != null) tutorial.Open();
        }

        private void OnTutorialClosed()
        {
            if (EventSystem.current != null && tutorialButton != null)
                EventSystem.current.SetSelectedGameObject(tutorialButton.gameObject);
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
            if (tutorialButton != null) tutorialButton.onClick.RemoveListener(OpenTutorial);
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
