using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleClass : MonoBehaviour
{
    void Start()
    {
        // Only specifying the sceneName or sceneBuildIndex will load the scene with the Single mode
        SceneManager.LoadScene("Beach", LoadSceneMode.Additive);
    }
}