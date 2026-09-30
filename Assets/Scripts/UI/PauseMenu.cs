using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SkySteps.UI
{
    /// <summary>
    /// Toggles the in-level pause panel from the Pause action. While paused, time is frozen, all
    /// audio is muted, and the listed gameplay behaviours stop reading input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference pauseAction;

        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button quitToTitleButton;

        [Header("Tutorial")]
        [Tooltip("Opened over the pause panel. The game stays paused while it is up.")]
        [SerializeField] private TutorialPageSwitcher tutorial;

        [Header("Level")]
        [Tooltip("Pausing is blocked while this popup is up; it already freezes the game.")]
        [SerializeField] private ResultScreen resultScreen;
        [Tooltip("Stopped while paused, so the player does not turn or buffer a jump behind the menu.")]
        [SerializeField] private Behaviour[] haltedWhilePaused;
        [SerializeField] private string titleSceneName = "Title";

        private InputAction _pause;
        private bool _enabledPause;
        private Coroutine _resumeRoutine;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (pauseAction == null || panel == null || resumeButton == null || restartButton == null ||
                quitToTitleButton == null)
            {
                Debug.LogError($"{nameof(PauseMenu)}: pause action, panel and all three buttons must be assigned.", this);
                enabled = false;
                return;
            }

            _pause = pauseAction.action;
            panel.SetActive(false);

            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(SceneLoader.ReloadCurrent);
            quitToTitleButton.onClick.AddListener(QuitToTitle);
            if (tutorialButton != null) tutorialButton.onClick.AddListener(OpenTutorial);
        }

        private void OnEnable()
        {
            if (tutorial != null) tutorial.Closed += OnTutorialClosed;

            // Same rule as the player's input: only switch on what is off, never the whole map.
            _enabledPause = _pause != null && !_pause.enabled;
            if (_enabledPause) _pause.Enable();
        }

        private void OnDisable()
        {
            if (tutorial != null) tutorial.Closed -= OnTutorialClosed;
            if (_enabledPause) _pause.Disable();
            _enabledPause = false;
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
            if (restartButton != null) restartButton.onClick.RemoveListener(SceneLoader.ReloadCurrent);
            if (quitToTitleButton != null) quitToTitleButton.onClick.RemoveListener(QuitToTitle);
            if (tutorialButton != null) tutorialButton.onClick.RemoveListener(OpenTutorial);
        }

        private void Update()
        {
            if (!_pause.WasPressedThisFrame()) return;

            // While the tutorial is up the key only backs out of it, and never reaches the pause state.
            if (tutorial != null && tutorial.IsOpen)
            {
                tutorial.Close();
                return;
            }

            if (IsPaused) Resume();
            else Pause();
        }

        private void OpenTutorial()
        {
            if (tutorial == null) return;

            // The pause panel steps aside so the tutorial has the screen; the game stays frozen.
            panel.SetActive(false);
            tutorial.Open();
        }

        private void OnTutorialClosed()
        {
            if (!IsPaused) return;

            panel.SetActive(true);
            if (EventSystem.current != null && tutorialButton != null)
                EventSystem.current.SetSelectedGameObject(tutorialButton.gameObject);
        }

        public void Pause()
        {
            if (IsPaused || (resultScreen != null && resultScreen.IsShowing)) return;

            if (_resumeRoutine != null)
            {
                StopCoroutine(_resumeRoutine);
                _resumeRoutine = null;
            }

            IsPaused = true;
            SetHalted(true);
            panel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;

            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            panel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;

            // Space both presses the Resume button and means Jump. Handing control back a frame
            // later keeps that same press from launching the player the moment the menu closes.
            _resumeRoutine = StartCoroutine(UnhaltNextFrame());
        }

        private void QuitToTitle()
        {
            SceneLoader.Load(titleSceneName);
        }

        private IEnumerator UnhaltNextFrame()
        {
            yield return null;
            SetHalted(false);
            _resumeRoutine = null;
        }

        private void SetHalted(bool halted)
        {
            if (haltedWhilePaused == null) return;

            for (int i = 0; i < haltedWhilePaused.Length; i++)
            {
                if (haltedWhilePaused[i] != null) haltedWhilePaused[i].enabled = !halted;
            }
        }
    }
}
