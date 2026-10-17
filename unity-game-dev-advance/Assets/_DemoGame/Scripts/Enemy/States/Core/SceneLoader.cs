using UnityEngine.SceneManagement;

namespace DemoGame
{
    public static class SceneLoader
    {
        public static void LoadTitle()
        {
            SceneManager.LoadScene(SceneNames.Title);
        }

        public static void LoadInGame()
        {
            SceneManager.LoadScene(SceneNames.InGame);
        }

        public static void ReloadCurrent()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
