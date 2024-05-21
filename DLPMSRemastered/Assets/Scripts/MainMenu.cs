using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void MainMenuScene()
    {
        Invoke("LoadMainMenuScene", 5);
    }

    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
