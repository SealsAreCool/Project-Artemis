using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneClick : MonoBehaviour
{
    [Tooltip("scene to load")]
    public string sceneToLoad = "MainGame"; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
