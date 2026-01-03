using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{

    public string gameSceneName = "MainGame";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
