using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkySteps.UI
{
    /// <summary>
    /// The single place scenes are changed from. Pausing and the result popup freeze time and mute
    /// audio globally, and those settings survive a scene load, so every load resets them first.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>Loads a scene by name, e.g. the title or a level.</summary>
        public static void Load(string sceneName)
        {
            ResetGlobalState();
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>Reloads the active scene from the start.</summary>
        public static void ReloadCurrent()
        {
            ResetGlobalState();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static void ResetGlobalState()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
