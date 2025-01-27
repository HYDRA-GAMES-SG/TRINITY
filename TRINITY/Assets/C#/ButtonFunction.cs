using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunction : MonoBehaviour
{
    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
    public void StartMenu(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
    public void OnQuit()
    {
        Application.Quit();
    }
}
