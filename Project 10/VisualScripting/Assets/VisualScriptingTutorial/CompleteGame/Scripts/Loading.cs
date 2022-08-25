using UnityEngine;
using UnityEngine.SceneManagement;
using VisualScriptingTutorial;

/// <summary>
/// This is added in the first scene that is loading in a Build (scene called Loading) and initialize all data needed
/// </summary>
public class Loading : MonoBehaviour
{
    void Awake()
    {
        EntryPoint.Create();
        EntryPoint.Init();
        SceneManager.LoadScene(1);
    }
}
